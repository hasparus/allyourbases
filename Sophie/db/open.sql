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

create or replace function get_participant
  (lo varchar, ph varchar)
    returns int as
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
    returns int as
$$
declare
  p int;
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
  (login varchar, password varchar, talk_id integer, _rating integer)
$$
declare
  p int;
begin
  select get_participant(login, password)
    into p;

  if p is not null then
    insert into rating
      values (talk_id, p, _rating, default);
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
  speaker_id int;

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
      insert into talk
        values (default, _title, ename, speaker_id, start_ts, _room, null, null, now());
    else
      update talk
        set (title, event_name, room, start_timestamp, accepted_timestamp) 
          = (_title, ename, _room, start_ts, now())
        where id = talk_id;
    end if;

    select rate(org_login, org_password, talk_id, init_eval);
    return 1;
  end if;
  return 0;
end
$$ language plpgsql;

--(*u) attendance <login> <password> <talk> // odnotowanie faktycznej obecności uczestnika <login> na referacie <talk>
create or replace function note_attendance
  (login varchar, password varchar, talk_id integer)
    returns int as
$$
declare
  p int;
begin
  select get_participant(login, password)
    into p;

  if p is not null then
    insert into attendance
      values (p, talk_id, default);
    return 1;
  end if;
  return 0;
end  
$$ language plpgsql;

--(o) reject <login> <password> <talk> // usuwa referat spontaniczny <talk> z listy zaproponowanych,
create or replace function cancel_talk
  (login varchar, password varchar, talk_id integer)
$$
begin
  if authorize_organiser(org_login, org_password) then
    update talk
      set rejected_timestamp = now(); 
      where id = talk_id;
    return 1;
  end if;
  return 0;
end
$$ language plpgsql;

--(u) proposal  <login> <password> <talk> <title> <start_timestamp> // propozycja referatu spontanicznego, <talk> - unikalny identyfikator referatu
create or replace function propose_talk
  (login varchar, password varchar, talk_id integer, talk_title varchar, start_ts timestamp with time zone)
$$
declare
  p int;
begin
  select get_participant(login, password)
    into p;

  if p is not null then
    if talk_id is null then
      insert into talk
        values (default, talk_title, null, p, start_ts, null, now(), null, null);
    else
      update talk -- po coś w specyfikacji jest to <talk>, nie?
        set (title, start_timestamp) 
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
