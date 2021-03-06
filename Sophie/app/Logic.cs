﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using Sophie.DataLayer;
using Sophie.Utils;

namespace Sophie
{
    public static class Logic
    {
        private static DataAccess _dataAccess;

        public static JObject Call(string functionName, JObject parameters) =>
            Functions[functionName]?.Invoke(parameters).ToJObject() ??
            CallResult.NotImplemented.ToJObject();

        private static Func<JObject, CallResult> SqlProcWrapper(
            string procName, params string[] parameters)
        {
            return args =>
            {
                if (!ValidCallParameters(args, parameters))
                    return CallResult.Error(
                        $"Invalid parameters for {procName} sql function.");

                var actualParams = parameters.Select(
                    key => args[key].ToSqlString());

                return _dataAccess.ExecuteSqlFromString(
                    $"select * from {procName}("
                    + string.Join(", ", actualParams)
                    + ");");
            };
        }

        private static CallResult EstabilishConnection(JObject parameters)
        {
            var pms = AuthorizeConnect(parameters);
            if (pms == null)
                return CallResult.Error("Method EstabilishConnection got wrong parameters.");
            (string dbName, string login, string password) = pms.Value;

            _dataAccess = new DataAccess(
                $"Host=localhost;Username={login};Password={password};Database={dbName}");
            try
            {
                var connection = _dataAccess.ProvideConnection();
                connection.Open();

                return connection.IsOpen()
                    ? CallResult.Ok
                    : CallResult.Error("Coudn't open connection to database.");
            }
            catch (PostgresException e)
            {
                return CallResult.Error(e.Message);
            }
            catch (SocketException)
            {
                Debug.Log("Please enable socket access to PostgreSQL server.");
                throw;
            }
        }

        private static CallResult Setup(JObject parameters)
        {
            if (AuthorizeConnect(parameters) == null)
                return CallResult.Error("Method Setup got wrong parameters.");

            try
            {
                var connection = _dataAccess.ProvideConnection();
                var res = _dataAccess.ExecuteSqlFromFile("db/open.sql", connection);
                if (res.Data != null && res.Data.Count == 0)
                {   //WORKAROUND, because it will be diff tested probably ;c
                    return CallResult.Ok;
                }
                return res;
            }
            catch (PostgresException e)
            {
                return CallResult.Error(e.Message);
            }
            catch (SocketException)
            {
                Debug.Log("Please enable socket access to PostgreSQL server.");
                throw;
            }
        }

        private static CallResult Open(JObject parameters)
        {
            var est = EstabilishConnection(parameters);
            return est == CallResult.Ok ? Setup(parameters) : est;
        }

        private static (string, string, string)? AuthorizeConnect(JObject parameters)
        {
            if (!ValidCallParameters(parameters, new[] { "baza", "login", "password" }))
                return null;

            return (parameters["baza"].ToSqlString(),
                    parameters["login"].ToSqlString(),
                    parameters["password"].ToSqlString());
        }

        private static bool ValidCallParameters(JObject given, IEnumerable<string> requested)
            => requested.All(p => given[p] != null);

        private static readonly ReferenceHash<string, Func<JObject, CallResult>> Functions =
            new ReferenceHash<string, Func<JObject, CallResult>>
            {
            {"open", Open},
            {"connect", EstabilishConnection},
            {"setup", Setup},
            {"drop_the_base", args =>
                {
                    if (_dataAccess?.ProvideConnection() == null)
                        return CallResult.Error("Can't drop if connection isn't estabilished");
                    if (!ValidCallParameters(args, new[] {"secret"})
                        || args["secret"].ToString() != "42")
                        return CallResult.Error($"{args["secret"].ToSqlString()} is not the secret.");
                    return _dataAccess.ExecuteSqlFromFile("db/drop.sql");
                }
            },
            {"organizer",
                SqlProcWrapper("add_organiser", "secret", "newlogin", "newpassword")
            },
            {"user",
                SqlProcWrapper("add_participant", "login", "password", "newlogin", "newpassword")
            },
            {"event",
                SqlProcWrapper("add_event", "login", "password", "eventname",
                    "start_timestamp", "end_timestamp")
            },
            {"talk",
                SqlProcWrapper("add_talk", "login", "password", "speakerlogin",
                    "talk", "title", "start_timestamp", "room", "initial_evaluation", "eventname")
            },
            {"register_user_for_event",
                SqlProcWrapper("register_user_for_event", "login", "password", "eventname")
            },
            {"attendance",
                SqlProcWrapper("note_attendance", "login", "password", "talk")
            },
            {"evaluation",
                SqlProcWrapper("rate", "login", "password", "talk", "rating")
            },
            {"reject",
                SqlProcWrapper("cancel_talk", "login", "password", "talk")
            },
            {"proposal",
                SqlProcWrapper("propose_talk", "login", "password", "talk", "title", "start_timestamp")
            },
            {"friends",
                SqlProcWrapper("declare_friendship", "login1", "password", "login2")
            },
            {"user_plan",
                SqlProcWrapper("get_user_plan", "login", "limit")
            },
            {"day_plan",
                SqlProcWrapper("get_day_plan", "timestamp")
            },
            {"best_talks",
                SqlProcWrapper("get_best_talks", "start_timestamp", "end_timestamp", "limit", "all")
            },
            {"most_popular_talks",
                SqlProcWrapper("get_most_popular_talks", "start_timestamp", "end_timestamp", "limit")
            },
            {"attended_talks",
                SqlProcWrapper("get_attended_talks", "login", "password")
            },
            {"abandoned_talks",
                SqlProcWrapper("get_abandoned_talks", "login", "password", "limit")
            },
            {"recently_added_talks",
                SqlProcWrapper("get_recently_accepted_talks", "limit")
            },
            {"rejected_talks",
                SqlProcWrapper("get_rejected_talks", "login", "password")
            },
            {"proposals",
                SqlProcWrapper("get_proposals", "login", "password")
            },
            {"friends_talks",
                SqlProcWrapper("get_friends_talks", "login", "password", "start_timestamp", "end_timestamp", "limit")
            },
            {"friends_events",
                SqlProcWrapper("get_friends_on_event", "login", "password", "eventname")
            },
            {"recommended_talks", args =>
                {
                    if (!ValidCallParameters(args, new[] {"login", "password", "start_timestamp", "end_timestamp", "limit"}))
                        return CallResult.Error("Wrong arguments for recommended talks.");

                    var callResult = _dataAccess.ExecuteSqlFromString(
                        $"select * from participant where login = {args["login"].ToSqlString()} and password_hash = {args["password"].ToSqlString()}");

                    // Debug.Log("request author: ");
                    // Debug.Log(callResult.ToString());

                    var limit = args["limit"].Value<long>() == 0
                        ? long.MaxValue
                        : args["limit"].Value<long>();

                    CallResult result = new CallResult(CallResult.Status.Error);
                    if (callResult.Data != null && callResult.Data.Count > 0)
                    {
                        var user = callResult.Data[0];

                        if (user["login"].Value<string>() != args["login"].Value<string>())
                        {
                            // Debug.Log(user["login"].ToString());
                            // Debug.Log(args["login"].ToString());
                            throw new ArgumentException("Database is lying to me.");
                        }


                        var st = args["start_timestamp"].ToSqlString();
                        var et = args["end_timestamp"].ToSqlString();

                        var ratings = _dataAccess.ExecuteSqlFromString(
                            $@"select t.id talk, sum(r.rating) sum, count(r.rating) count
                                from talk t
                                join rating r on (t.id = r.talk_id)
                                where t.start_timestamp >= {st}
                                    and t.start_timestamp <= {et}
                                    and t.rejected_timestamp is null
                                    and t.accepted_timestamp is not null
                                group by t.id;");

                        var attendances = _dataAccess.ExecuteSqlFromString(
                            $@"select t.id talk, count(*) attendances
                                from talk t
                                join attendance a on (t.id = a.talk_id)
                                where t.start_timestamp >= {st}
                                    and t.start_timestamp <= {et}
                                    and t.rejected_timestamp is null
                                    and t.accepted_timestamp is not null
                                group by t.id;");

                        var attendeeRatings = _dataAccess.ExecuteSqlFromString(
                            $@"select t.id talk, sum(r.rating) sum, count(r.rating) count
                                from talk t
                                join rating r on (t.id = r.talk_id)
                                join attendance using (talk_id, participant_id) 
                                where t.start_timestamp >= {st}
                                  and t.start_timestamp <= {et}
                                  and t.rejected_timestamp is null
                                  and t.accepted_timestamp is not null
                                group by t.id");

                        const int friendshipBonus = 20;
                        var friendBonuses = _dataAccess.ExecuteSqlFromString(
                            $@"select t.id talk, {friendshipBonus} bonus
                                from talk t
                                join friendship_declaration fd on (t.speaker_id = fd.receiver_id)
                                where t.start_timestamp >= {st}
                                  and t.start_timestamp <= {et}
                                  and t.rejected_timestamp is null
                                  and t.accepted_timestamp is not null
                                  and fd.declarer_id = {user["id"]}");

                        //Debug.Log("---");
                        //Debug.Log(ratings.ToString());
                        //Debug.Log(attendances.ToString());
                        //Debug.Log(attendeeRatings.ToString());
                        //Debug.Log(friendBonuses.ToString());

                        var rat = Dearrayify(ratings.Data);
                        var att = Dearrayify(attendances.Data);
                        var atr = Dearrayify(attendeeRatings.Data);
                        var frb = Dearrayify(friendBonuses.Data);

                        result = _dataAccess.ExecuteSqlFromString(
                            $@"select t.id talk, speaker.login speakerlogin, t.start_timestamp, t.title, t.room
                                from talk t
                                join participant speaker on (t.speaker_id = speaker.id)
                                where t.start_timestamp >= {st}
                                  and t.start_timestamp <= {et}
                                  and t.rejected_timestamp is null
                                  and t.accepted_timestamp is not null;");

                        //Debug.Log(result.ToString());

                        if (result.State == CallResult.Status.Error)
                            return CallResult.Error("Score failed.");

                        foreach (var jToken in result.Data)
                        {
                            var jo = (JObject) jToken;
                            jo.Add("score", Score(jo["talk"].Value<string>(), att, rat, atr, frb));
                        }
                        //Debug.Log("---");

                    }
                    
                    if (result.Data != null)
                        result = new CallResult(CallResult.Status.Ok,
                            new JArray(result.Data.OrderByDescending(x => x["score"].Value<int>()).Take(Convert.ToInt32(Math.Min(limit, int.MaxValue)))));
                    return result;
                }
            }
            };

        private static int Score
            (string talk, JObject attendances, JObject ratings, JObject attendeeRatings, JObject friendBonuses)
        { // ta metoda jest ohydna
            try
            {
                const int middleScore = 5;
                int att;
                try
                {
                    att = attendances[talk] != null
                        ? attendances[talk]["attendances"].Value<int?>() ?? 0
                        : 0;
                }
                catch
                {
                    att = 0;
                }

                int ratingSum;
                try
                {
                    ratingSum = ratings[talk] != null
                        ? ratings[talk]["sum"].Value<int?>() ?? 0
                        : 0;
                }
                catch
                {
                    ratingSum = 0;
                }

                int ratingCount;
                try
                {
                    ratingCount = ratings[talk] != null
                        ? ratings[talk]["count"].Value<int?>() ?? 0
                        : 0;
                }
                catch
                {
                    ratingCount = 0;
                }

                int aRatingSum;
                try
                {
                    aRatingSum = ratings[talk] != null
                        ? attendeeRatings[talk]["sum"].Value<int?>() ?? 0
                        : 0;
                }
                catch
                {
                    aRatingSum = 0;
                }
                int aRatingCount;
                try
                {
                    aRatingCount = ratings[talk] != null
                        ? attendeeRatings[talk]["count"].Value<int?>() ?? 0
                        : 0;
                }
                catch
                {
                    aRatingCount = 0;
                }
                int fBonus;
                try
                {
                    fBonus = friendBonuses[talk] != null
                        ? friendBonuses[talk]["bonus"].Value<int?>() ?? 0
                        : 0;
                }
                catch
                {
                    fBonus = 0;
                }
                
                return att + ratingSum - middleScore * ratingCount + aRatingSum - middleScore * aRatingCount + fBonus;
            }
            catch (JsonException)
            {
                return 0;
            }
        }

        private static JObject Dearrayify(JArray jarr)
        {
            JObject res = new JObject();
            if (jarr != null)
                foreach (var jToken in jarr)
                {
                    var jo = jToken as JObject;
                    if (jo == null) continue;
                    res.Add(jo["talk"].Value<string>(), jo);
                }
            return res;
        }
    }
}