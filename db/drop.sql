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