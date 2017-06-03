-- Piotr Pietrzak 274620, model fizyczny.

-- notatka:
-- "different from other database systems, 
-- in postgresql, there is no performance 
-- difference among three character types.
-- in most situation, you should use text 
-- or varchar, and varchar(n) if you want
-- postgresql to check for the length limit."
-- 

create table if not exists participant (
  id                    serial                    primary key,
  login                 varchar                   unique not null,
  password_hash         varchar                   not null,
  first_name            varchar,
  last_name             varchar,
  birthday              date,
  gender                varchar,
  joined_timestamp      timestamp with time zone  default now()
);

create unique index if not exists participant_login_index on participant (login);

create table if not exists organiser (
  id                    integer                   references participant on delete cascade,
  login                 varchar                   unique not null,
  password_hash         varchar                   not null
);

create unique index if not exists organiser_login_index on organiser (login);

create table if not exists friendship_declaration (
  declarer_id           integer                   references participant on delete cascade,
  receiver_id           integer                   references participant on delete cascade,
  timestamp             timestamp                 not null default now(),
  primary key (declarer_id, receiver_id)
);

create table if not exists event (  
  event_name            varchar                   primary key,
  start_timestamp       timestamp with time zone  not null,
  end_timestamp         timestamp with time zone  not null
);

create table if not exists registration (
  event_name            varchar                   references event on delete cascade,
  participant_id        integer                   references participant on delete cascade,
  created_timestamp     timestamp with time zone  not null default now(),
  primary key (participant_id, event_name)
);

create table if not exists talk (
  id                    serial                    primary key,
  title                 varchar                   not null,
  event_name            varchar                   references event on delete cascade,
  speaker_id            integer                   references participant on delete cascade,
  start_timestamp       timestamp with time zone  not null default now(),
  room                  varchar,
  proposed_timestamp    timestamp with time zone,
  rejected_timestamp    timestamp with time zone,
  accepted_timestamp    timestamp with time zone
);

-- zakładam, że użytkownik będzie mógł wyszukiwać referaty po tytule
create index if not exists title_index on talk (title);

create table if not exists rating (
  talk_id               integer                   references talk on delete cascade,
  participant_id        integer                   references participant on delete cascade,
  rating                integer                   check (rating >= 0 and rating <= 10),
  created_timestamp     timestamp with time zone  not null default now(),
  primary key (talk_id, participant_id)
);

create table if not exists attendance (
  talk_id               integer                   references talk on delete cascade,
  participant_id        integer                   references participant on delete cascade,
  created_timestamp     timestamp with time zone  not null default now(),
  primary key (talk_id, participant_id)
);

------------------------------------------------------------------------
-- functions:

create or replace function add_organiser
  (secret varchar, lo varchar, ph varchar) 
    returns integer as
$$
begin
  if secret = 'd8578edf8458ce06fbc5bb76a58c5ca4' then
    insert into participant (login, password_hash)
      values (lo, ph);

    insert into organiser (id, login, password_hash)
      select id, lo, ph
      from participant
      where participant.login = lo;

    return 1;
  end if;
  return 0;
end
$$ language plpgsql;

---

create or replace function authorize_organiser
  (lo varchar, ph varchar)
    returns boolean as
$$

  select (id is not null)
    from organiser
    where lo = organiser.login
      and ph = organiser.password_hash;

$$ language sql;

---

create or replace function get_participant
  (lo varchar, ph varchar)
    returns integer as
$$
  select id
    from participant p
    where lo = p.login
      and ph = p.password_hash;
$$ language sql;

create or replace function authorize_participant
  (lo varchar, ph varchar)
    returns boolean as
$$
  select get_participant(lo, ph) is not null;
$$ language sql;

---

create or replace function add_participant
  (org_login varchar, org_password varchar, new_user_login varchar, new_user_password varchar)
    returns integer as
$$
begin
  if authorize_organiser(org_login, org_password) then
    insert into participant (login, password_hash)
      values (new_user_login, new_user_password);
    return 1;
  end if;
  return 0;
end
$$ language plpgsql;

---

create or replace function add_event
  (org_login varchar, org_password varchar, ename varchar,
  start_ts timestamp with time zone, end_ts timestamp with time zone)
    returns integer as
$$
begin
  if authorize_organiser(org_login, org_password) then
    insert into event
      values (ename, start_ts, end_ts);
    return 1;
  end if;
  return 0;
end
$$ language plpgsql;

---

create or replace function register_user_for_event
  (login varchar, password varchar, ename varchar)
    returns integer as
$$
declare
  p integer;
begin
  select get_participant(login, password)
    into p;

  if p is null then
    return 0;
  end if;

  insert into registration
    values (ename, p, now())
    on conflict (event_name, participant_id) do nothing;
  return 1;
end
$$ language plpgsql;

---

--(*u) evaluation <login> <password> <talk> <rating> // ocena referatu <talk> w skali 0-10 przez uczestnika <login>
create or replace function rate
  (login varchar, password varchar, _talk_id integer, _rating integer)
    returns integer as
$$
declare
  p integer;
begin
  select get_participant(login, password)
    into p;

  if p is not null then
    insert into rating
      values (_talk_id, p, _rating, default)
      on conflict (talk_id, participant_id) 
        do update set rating = _rating;
    return 1;
  end if;
  return 0;
end
$$ language plpgsql;

---

create or replace function add_talk
  (org_login varchar, org_password varchar, speaker varchar, talk_id integer, _title varchar,
  start_ts timestamp with time zone, _room varchar, init_eval integer, ename varchar)
    returns int as
$$
declare
  speaker_id integer;

begin
  if authorize_organiser(org_login, org_password) then
    if ename = '' then
      ename = null;
    end if;

    select id from participant
      into speaker_id
      where login = speaker
      limit 1;
      
    if talk_id is null then
      with inserted as (
        insert into talk
          values (default, _title, ename, speaker_id, start_ts, _room, null, null, now())
          returning id )
      select id from inserted
        into talk_id;
    else
      update talk
        set (title, event_name, room, start_timestamp, accepted_timestamp) 
          = (_title, ename, _room, start_ts, now())
        where id = talk_id;
    end if;

    return rate(org_login, org_password, talk_id, init_eval);
  end if;
  return 0;
end
$$ language plpgsql;

--(*u) attendance <login> <password> <talk> // odnotowanie faktycznej obecności uczestnika <login> na referacie <talk>
create or replace function note_attendance
  (login varchar, password varchar, talk_id integer)
    returns integer as
$$
declare
  p integer;
begin
  select get_participant(login, password)
    into p;

  if p is not null then
    insert into attendance
      values (talk_id, p, default);
    return 1;
  end if;
  return 0;
end  
$$ language plpgsql;

--(o) reject <login> <password> <talk> // usuwa referat spontaniczny <talk> z listy zaproponowanych,
create or replace function cancel_talk
  (login varchar, password varchar, talk_id integer)
    returns int as
$$
begin
  if authorize_organiser(login, password) then
    update talk
      set rejected_timestamp = now()
        where id = talk_id;
    return 1;
  end if;
  return 0;
end
$$ language plpgsql;

--(u) proposal  <login> <password> <talk> <title> <start_timestamp> // propozycja referatu spontanicznego, <talk> - unikalny identyfikator referatu
create or replace function propose_talk
  (login varchar, password varchar, talk_id integer, talk_title varchar, start_ts timestamp with time zone)
    returns integer as
$$
declare
  p integer;
begin
  select get_participant(login, password)
    into p;

  if p is not null then
    if talk_id is null then
      insert into talk
        values (default, talk_title, null, p, start_ts, null, now(), null, null);
    else
      update talk -- po coś w specyfikacji jest to <talk>, nie?
        set (title, start_timestamp, proposed_timestamp) 
          = (talk_title, start_ts, now())
        where id = talk_id
              and speaker_id = p;
    end if;
    return 1;
  end if;
  return 0;
end
$$ language plpgsql;

--(u) friends <login1> <password> <login2> // uczestnik <login1> chce nawiązać znajomość z uczestnikiem <login2>, znajomość uznajemy za nawiązaną jeśli obaj uczestnicy chcą ją nawiązać tj. po wywołaniach friends <login1> <password1> <login2> i friends <login2> <password2> <login1>
create or replace function declare_friendship
  (_login varchar, _password varchar, _target varchar)
    returns integer as
$$
declare
  target_id integer;
begin
  select id 
    into target_id
    from participant
    where login = _target;

  insert into friendship_declaration
    values (get_participant(_login, _password), target_id)
    on conflict do nothing;

  return 1;
end
$$ language plpgsql;

---



--(*N) user_plan <login> <limit>
-- // zwraca plan najbliższych referatów z wydarzeń, na które dany uczestnik jest zapisany 
-- (wg rejestracji na wydarzenia) posortowany wg czasu rozpoczęcia, wypisuje pierwsze <limit> referatów, przy czym 0 oznacza,
-- że należy wypisać wszystkie
--// Atrybuty zwracanych krotek: 
--   <login> <talk> <start_timestamp> <title> <room>

create or replace function get_user_plan
  (_login varchar, _limit bigint)
    returns table (login varchar, talk integer, start_timestamp timestamp with time zone, title varchar, room varchar) as
$$
begin
  if _limit = 0 then
    _limit = 9223372036854775807;
  end if;

  return query 
    select p.login, talk.id, talk.start_timestamp, talk.title, talk.room from
      participant p
      join registration on (p.id = registration.participant_id)
      join talk using (event_name)
      where p.login = _login
        and talk.rejected_timestamp is null
        and talk.accepted_timestamp is not null
      order by start_timestamp
      limit _limit;

end
$$ language plpgsql;

--(*N) day_plan <timestamp> 
-- // zwraca listę wszystkich referatów zaplanowanych na dany dzień posortowaną rosnąco wg sal,
--    w drugiej kolejności wg czasu rozpoczęcia
--//  <talk> <start_timestamp> <title> <room>
create or replace function get_day_plan
  (daystamp timestamp with time zone)
    returns table (talk integer, start_timestamp timestamp with time zone, title varchar, room varchar) as
$$
begin

  return query 
    select t.id, t.start_timestamp, t.title, t.room from
      talk t
      where extract(day from t.start_timestamp) = extract(day from daystamp)
        and t.accepted_timestamp is not null
        and t.rejected_timestamp is null
      order by t.room, t.start_timestamp;

end
$$ language plpgsql;

--(*N) best_talks <start_timestamp> <end_timestamp> <limit> <all> 
--// zwraca referaty rozpoczynające się w  danym przedziale czasowym
--   posortowane malejąco wg średniej oceny uczestników, 
--   przy czym jeśli <all> jest równe 1 należy wziąć pod uwagę wszystkie oceny, 
--   w przeciwnym przypadku tylko oceny uczestników, którzy byli na referacie obecni,
--   wypisuje pierwsze <limit> referatów, przy czym 0 oznacza, że należy wypisać wszystkie
--//  <talk> <start_timestamp> <title> <room>
create or replace function get_best_talks
  (_start_timestamp timestamp with time zone, _end_timestamp timestamp with time zone, _limit bigint, _all integer)
    returns table (talk integer, start_timestamp timestamp with time zone, title varchar, room varchar) as
$$
begin
  if _limit = 0 then
    _limit = 9223372036854775807;
  end if;

  if _all = 1 then
    return query
      select t.id, t.start_timestamp, t.title, t.room from
        talk t
        join rating r on (t.id = r.talk_id)
        where t.start_timestamp >= _start_timestamp
          and t.start_timestamp <= _end_timestamp
          and t.rejected_timestamp is null
          and t.accepted_timestamp is not null
        group by t.id
        order by avg(r.rating) desc 
        limit _limit;
  else
    return query
      select t.id, t.start_timestamp, t.title, t.room from
        talk t
        join rating r on (t.id = r.talk_id)
        join attendance using (talk_id, participant_id) 
        where t.start_timestamp >= _start_timestamp
          and t.start_timestamp <= _end_timestamp
          and t.rejected_timestamp is null
          and t.accepted_timestamp is not null
        group by t.id
        order by avg(r.rating) desc 
        limit _limit;
  end if;
end
$$ language plpgsql;

--(*N) most_popular_talks <start_timestamp> <end_timestamp> <limit> 
--// zwraca referaty rozpoczynające się w podanym przedziału czasowego posortowane malejąco wg obecności, 
--   wypisuje pierwsze <limit> referatów, przy czym 0 oznacza, że należy wypisać wszystkie
--//  <talk> <start_timestamp> <title> <room>
create or replace function get_most_popular_talks
  (_start_timestamp timestamp with time zone, _end_timestamp timestamp with time zone, _limit bigint)
    returns table (talk integer, start_timestamp timestamp with time zone, title varchar, room varchar) as
$$
begin
  if _limit = 0 then
    _limit = 9223372036854775807;
  end if;

  return query
    select t.id, t.start_timestamp, t.title, t.room from
      talk t
      join attendance on (t.id = attendance.talk_id) 
      where t.start_timestamp >= _start_timestamp
        and t.start_timestamp <= _end_timestamp
        and t.rejected_timestamp is null
      group by t.id
      order by count(participant_id) desc 
      limit _limit;
end
$$ language plpgsql;


--(*U) attended_talks <login> <password> 
--// zwraca dla danego uczestnika referaty, na których był obecny 
--//  <talk> <start_timestamp> <title> <room>

create or replace function get_attended_talks
  (login varchar, password varchar)
    returns table (talk integer, start_timestamp timestamp with time zone, title varchar, room varchar) as
$$
declare
  p integer;
begin
  select get_participant(login, password)
    into p;

  if p is null then
    return query select null;
  end if;

  return query
    select t.id, t.start_timestamp, t.title, t.room from
      talk t
      join attendance a on (t.id = a.talk_id)
      where t.rejected_timestamp is null
        and t.accepted_timestamp is not null
        and participant_id = p;
end
$$ language plpgsql;

--(*O) abandoned_talks <login> <password>  <limit> 
--// zwraca listę referatów posortowaną malejąco wg liczby uczestników <number> 
-- zarejestrowanych na wydarzenie obejmujące referat, którzy nie byli na tym referacie obecni, 
-- wypisuje pierwsze <limit> referatów, przy czym 0 oznacza, że należy wypisać wszystkie
--//  <talk> <start_timestamp> <title> <room> <number>

create or replace function get_abandoned_talks
  (_login varchar, _password varchar, _limit bigint)
    returns table (talk integer, start_timestamp timestamp with time zone, title varchar, room varchar, number bigint) as
$$
begin
  if authorize_organiser(_login, _password) then

    if _limit = 0 then
      _limit = 9223372036854775807;
    end if;

    return query 
      select xx.id talk, xx.st start_timestamp, xx.title title, xx.room room, count(xx.betrayer) number from (
        select t.id, t.start_timestamp, t.title, t.room, p.id from
        participant p
        join registration r on (p.id = r.participant_id)
        join talk t on (t.event_name = r.event_name)
        except (
          select a.talk_id, t.start_timestamp, t.title, t.room, p.id from
            participant p
            join attendance a on (p.id = a.participant_id)
            join talk t on (t.id = a.talk_id)
        ) 
      ) xx (id, st, title, room, betrayer)
      group by (xx.id, xx.st, xx.title, xx.room)
      order by count(xx.betrayer) desc
      limit _limit;

    return;
  
  end if;
  raise 'Organiser authorization fail.';
end
$$ language plpgsql;

--(N) recently_added_talks <limit> 
-- // zwraca listę ostatnio zarejestrowanych referatów,
--  wypisuje ostatnie <limit> referatów wg daty zarejestrowania, przy czym 0 oznacza, że należy wypisać wszystkie
--//  <talk> <speakerlogin> <start_timestamp> <title> <room>

create or replace function get_recently_accepted_talks
  (_limit bigint)
    returns table (talk integer, speakerlogin varchar, start_timestamp timestamp with time zone, title varchar, room varchar) as
$$
begin
  if _limit = 0 then
    _limit = 9223372036854775807;
  end if;

  return query
    select t.id, p.login, t.start_timestamp, t.title, t.room from
      talk t
      join participant p on (t.speaker_id = p.id)
      where t.accepted_timestamp is not null
        and t.rejected_timestamp is null
      order by t.accepted_timestamp desc
      limit _limit;

end
$$ language plpgsql;

--(U/O) rejected_talks <login> <password> 
--// jeśli wywołujący ma uprawnienia organizatora zwraca listę wszystkich odrzuconych referatów spontanicznych,
--   w przeciwnym przypadku listę odrzuconych referatów wywołującego ją uczestnika 
--//  <talk> <speakerlogin> <start_timestamp> <title>

create or replace function get_rejected_talks
  (_login varchar, _password varchar)
    returns table (talk integer, speakerlogin varchar, start_timestamp timestamp with time zone, title varchar) as
$$
declare
  _p integer;
begin
  if authorize_organiser(_login, _password) then

    return query
      select t.id, p.login, t.start_timestamp, t.title from
        talk t
        join participant p on (t.speaker_id = p.id)
        where t.rejected_timestamp is not null
        order by t.rejected_timestamp desc;
  
    return;
  else 
    select get_participant(_login, _password)
      into _p;

    if _p is not null then
      return query
        select t.id, p.login, t.start_timestamp, t.title from
          talk t
          join participant p on (t.speaker_id = p.id)
          where t.rejected_timestamp is not null
            and p.id = _p
          order by t.rejected_timestamp desc;
      return;
    end if;
  end if;
  raise 'get_rejected_talks authorization fail.';
end
$$ language plpgsql;


--(O) proposals <login> <password> 
--// zwraca listę propozycji referatów spontanicznych do zatwierdzenia lub odrzucenia, 
--   zatwierdzenie lub odrzucenie referatu polega na wywołaniu przez organizatora funkcji talk lub reject z odpowiednimi parametrami
--//  <talk> <speakerlogin> <start_timestamp> <title>

create or replace function get_proposals
  (_login varchar, _password varchar)
    returns table (talk integer, speakerlogin varchar, start_timestamp timestamp with time zone, title varchar) as
$$
begin
  if authorize_organiser(_login, _password) then
    return query
      select t.id, p.login, t.start_timestamp, t.title from
        talk t
        join participant p on (t.speaker_id = p.id)
        where t.rejected_timestamp is null
          and t.accepted_timestamp is null
          and t.proposed_timestamp is not null
        order by t.proposed_timestamp desc;
    return;
  end if;
  raise 'get_proposals authorization fail.';
end
$$ language plpgsql;

--(U) friends_talks <login> <password> <start_timestamp> <end_timestamp> <limit> 
--// lista referatów  rozpoczynających się w podanym przedziale czasowym wygłaszanych przez znajomych danego
--    uczestnika posortowana wg czasu rozpoczęcia, wypisuje pierwsze <limit> referatów, przy czym 0 oznacza, że należy wypisać wszystkie
--//  <talk> <speakerlogin> <start_timestamp> <title> <room>

create or replace function get_friends_talks
  (_login varchar, _password varchar, _start_timestamp timestamp with time zone, _end_timestamp timestamp with time zone, _limit bigint)
    returns table (talk integer, speakerlogin varchar, start_timestamp timestamp with time zone, title varchar, room varchar) as
$$
declare
  _p integer;
begin
  select get_participant(_login, _password)
    into _p;

  if _limit = 0 then
    _limit = 9223372036854775807;
  end if;

  if _p is not null then
    return query
      select t.id, p.login, t.start_timestamp, t.title, t.room from
        talk t
        join participant p on (t.speaker_id = p.id)
        join ( select distinct x.declarer_id x, y.declarer_id y from 
          friendship_declaration x, friendship_declaration y
          where x.declarer_id = y.receiver_id
            and y.declarer_id = x.receiver_id
        ) bffs on (bffs.x = _p)
        where t.rejected_timestamp is null
          and t.accepted_timestamp is not null
          and bffs.y = p.id
          and t.start_timestamp >= _start_timestamp
          and t.start_timestamp <= _end_timestamp
        order by t.start_timestamp
        limit _limit;
    return;
  end if;
  raise 'get_friends_talks authorization fail.';
end
$$ language plpgsql;

--(U) friends_events <login> <password> <eventname> // lista znajomych uczestniczących w danym wydarzeniu
--//  <login> <eventname> <friendlogin> 

create or replace function get_friends_on_event
  (_login varchar, _password varchar, _eventname varchar)
    returns table (login varchar, eventname varchar, friendlogin varchar) as
$$
declare
  _p integer;
begin
  select get_participant(_login, _password)
    into _p;

  if _p is not null then
    return query
      select i.login, event_name, friend.login from
        participant i
        join ( select distinct x.declarer_id x, y.declarer_id y from 
          friendship_declaration x, friendship_declaration y
          where x.declarer_id = y.receiver_id
            and y.declarer_id = x.receiver_id
        ) bffs on (bffs.x = _p)
        join participant friend on (bffs.y = friend.id)
        join registration r on (r.event_name = _eventname and r.participant_id = friend.id)
        where i.id = _p;
    return;
  end if;
  raise 'get_friends_on_event authorization fail.';
end
$$ language plpgsql;

--(U) recommended_talks <login> <password> <start_timestamp> <end_timestamp> <limit> 
--// zwraca referaty rozpoczynające się w podanym przedziale czasowym, 
-- które mogą zainteresować danego uczestnika (zaproponuj parametr <score> obliczany na podstawie dostępnych danych 
-- - ocen, obecności, znajomości itp.), wypisuje pierwsze <limit> referatów wg nalepszego <score>, 
-- przy czym 0 oznacza, że należy wypisać wszystkie
--//  <talk> <speakerlogin> <start_timestamp> <title> <room> <score>
-- TODO: inside app