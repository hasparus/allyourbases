--
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

create index on participant (login);

create table if not exists organiser (
  id                    integer                   references participant on delete cascade,
  login                 varchar                   unique not null,
  password_hash         varchar                   not null
);

create index on organiser (login);

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
create index on talk (title);

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
    returns int as
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

create or replace function authorize_participant
  (lo varchar, ph varchar)
    returns boolean as
$$

  select (id is not null)
    from participant p
    where lo = p.login
      and ph = p.password_hash;

$$ language sql;


---

create or replace function add_participant
  (org_login varchar, org_password varchar, new_user_login varchar, new_user_password varchar)
    returns int as
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
    returns int as
$$
begin
  if authorize_organiser(org_login, org_password) THEN
    insert into event
      values (ename, start_ts, end_ts);
    return 1;
  end if;
  return 0;
end
$$ language plpgsql;