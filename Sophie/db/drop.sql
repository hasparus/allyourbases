drop table if exists rating;
drop table if exists attendance;
drop table if exists registration;
drop table if exists friendship_declaration;
drop table if exists talk;
drop table if exists event;
drop table if exists organiser;
drop table if exists participant;

drop function if exists add_organiser(varchar, varchar, varchar);
drop function if exists authorize_organiser(varchar, varchar);
drop function if exists add_participant(varchar, varchar, varchar, varchar);
drop function if exists authorize_participant(varchar, varchar);
drop function if exists add_event(varchar, varchar, varchar, timestamp with time zone, timestamp with time zone);
drop function if exists add_talk(varchar, varchar, varchar, varchar, varchar, timestamp with time zone, varchar, integer, varchar);
drop function if exists register_user_for_event(varchar, varchar, varchar);

drop function if exists rate(login varchar, password varchar, talk_id varchar, _rating integer);
drop function if exists note_attendance(varchar, varchar, varchar);
drop function if exists cancel_talk(varchar, varchar, varchar);
drop function if exists propose_talk(varchar, varchar, integer, varchar, timestamp with time zone);
drop function if exists declare_friendship(varchar, varchar, varchar);

drop function if exists get_user_plan(varchar, bigint);
drop function if exists get_day_plan(timestamp with time zone);
drop function if exists get_best_talks(timestamp with time zone, timestamp with time zone, bigint, integer);