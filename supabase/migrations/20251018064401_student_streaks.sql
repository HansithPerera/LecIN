drop policy "Allow Teachers to view students in course" on "public"."Students";

create extension if not exists "postgis" with schema "extensions";

create table "public"."Locations"
(
    "Id"        text                     not null,
    "Room"      text,
    "Level"     text                     not null,
    "Building"  text                     not null,
    "UpdatedAt" timestamp with time zone,
    "CreatedAt" timestamp with time zone not null default now(),
    "Coords"    extensions.geography     not null,
    "Address"   text
);


alter table "public"."Locations"
    enable row level security;

create table "public"."app_banners"
(
    "id"         uuid                     not null default gen_random_uuid(),
    "is_active"  boolean                  not null default false,
    "message"    text                     not null default 'Planned maintenance window'::text,
    "updated_at" timestamp with time zone not null default now()
);


alter table "public"."app_banners"
    enable row level security;

alter table "public"."Attendance"
    alter column "Timestamp" drop not null;

CREATE INDEX "LocationsGeoIndex" ON public."Locations" USING gist ("Coords");

CREATE UNIQUE INDEX "Locations_pkey" ON public."Locations" USING btree ("Id");

CREATE UNIQUE INDEX app_banners_pkey ON public.app_banners USING btree (id);

CREATE INDEX idx_attendance_student_class_time ON public."Attendance" USING btree ("StudentId", "ClassId", "Timestamp" DESC);

alter table "public"."Locations"
    add constraint "Locations_pkey" PRIMARY KEY using index "Locations_pkey";

alter table "public"."app_banners"
    add constraint "app_banners_pkey" PRIMARY KEY using index "app_banners_pkey";

alter table "public"."Classes"
    add constraint "Classes_Location_fkey" FOREIGN KEY ("Location") REFERENCES "Locations" ("Id") not valid;

alter table "public"."Classes"
    validate constraint "Classes_Location_fkey";

set check_function_bodies = off;

CREATE OR REPLACE FUNCTION public."CalculateStudentStreak"()
    RETURNS bigint
    LANGUAGE plpgsql
AS
$function$
DECLARE
    counter integer := 0;
    r       record;
BEGIN
    FOR r IN
        SELECT c."StartTime",
               CASE
                   WHEN a."StudentId" IS NULL THEN 1
                   ELSE 0
                   END AS is_absent
        FROM "Classes" c
                 LEFT JOIN "Attendance" a
                           ON a."ClassId" = c."Id"
                               AND a."StudentId" = auth.uid()::uuid
        WHERE c."StartTime" <= NOW()
        ORDER BY c."StartTime"
        LOOP
            IF r.is_absent = 1 THEN
                RETURN counter;
            ELSE
                counter := counter + 1;
            END IF;
        END LOOP;

    RETURN counter;
END;
$function$
;

create materialized view "public"."CourseStreaksAllTime" as
WITH "OrderedClasses" AS (SELECT c."Id" AS "ClassId",
                                 c."CourseCode",
                                 c."CourseYear",
                                 c."CourseSemesterCode"
                          FROM "Classes" c),
     "StudentAttendance" AS (SELECT s."Id"  AS "StudentId",
                                    oc."CourseCode",
                                    oc."CourseYear",
                                    oc."CourseSemesterCode",
                                    CASE
                                        WHEN (a."ClassId" IS NOT NULL) THEN 1
                                        ELSE 0
                                        END AS "IsPresent"
                             FROM (("OrderedClasses" oc
                                 CROSS JOIN "Students" s)
                                 LEFT JOIN "Attendance" a
                                   ON (((a."ClassId" = oc."ClassId") AND (a."StudentId" = s."Id")))))
SELECT sa."StudentId",
       st."FirstName",
       st."LastName",
       sa."CourseCode",
       sa."CourseYear",
       sa."CourseSemesterCode",
       CASE
           WHEN (sum(sa."IsPresent") = count(*)) THEN count(*)
           ELSE (0)::bigint
           END AS "StreakLength"
FROM ("StudentAttendance" sa
    JOIN "Students" st ON ((st."Id" = sa."StudentId")))
GROUP BY sa."StudentId", st."FirstName", st."LastName", sa."CourseCode", sa."CourseYear", sa."CourseSemesterCode";


create materialized view "public"."CourseStreaksMonthly" as
WITH "OrderedClasses" AS (SELECT c."Id" AS "ClassId",
                                 c."CourseCode",
                                 c."CourseYear",
                                 c."CourseSemesterCode"
                          FROM "Classes" c
                          WHERE (date_trunc('month'::text, c."StartTime") = date_trunc('month'::text, now()))),
     "StudentAttendance" AS (SELECT s."Id",
                                    oc."CourseCode",
                                    oc."CourseYear",
                                    oc."CourseSemesterCode",
                                    CASE
                                        WHEN (a."ClassId" IS NOT NULL) THEN 1
                                        ELSE 0
                                        END AS "IsPresent"
                             FROM (("OrderedClasses" oc
                                 CROSS JOIN "Students" s)
                                 LEFT JOIN "Attendance" a
                                   ON (((a."ClassId" = oc."ClassId") AND (a."StudentId" = s."Id")))))
SELECT "Id",
       "CourseCode",
       "CourseYear",
       "CourseSemesterCode",
       CASE
           WHEN (sum("IsPresent") = count(*)) THEN count(*)
           ELSE (0)::bigint
           END AS "StreakLength"
FROM "StudentAttendance"
GROUP BY "Id", "CourseCode", "CourseYear", "CourseSemesterCode";


CREATE OR REPLACE FUNCTION public."RefreshLeaderboard"()
    RETURNS void
    LANGUAGE plpgsql
    SECURITY DEFINER
AS
$function$
BEGIN
    REFRESH MATERIALIZED VIEW CONCURRENTLY public."CourseStreaksAllTime";
    REFRESH MATERIALIZED VIEW CONCURRENTLY public."CourseStreaksMonthly";
END;
$function$
;

CREATE OR REPLACE FUNCTION public."AdminHasPermission"(userid uuid, permname character varying)
    RETURNS boolean
    LANGUAGE plpgsql
    SECURITY DEFINER
AS
$function$
DECLARE
    perms     int;
    permValue int;
BEGIN
    SELECT "Permissions"
    INTO perms
    FROM "Admins"
    WHERE "Id" = userId;

    IF NOT FOUND THEN
        RETURN FALSE;
    END IF;

    SELECT "Value"
    INTO permValue
    FROM "PermissionFlags"
    WHERE "Name" ILIKE permName;

    RETURN (perms & permValue) = permValue;
END;
$function$
;

CREATE OR REPLACE FUNCTION public."IsAdmin"(userid uuid)
    RETURNS boolean
    LANGUAGE plpgsql
    SECURITY DEFINER
AS
$function$
BEGIN
    RETURN EXISTS (SELECT 1
                   FROM "Admins"
                   WHERE "Id" = userId);
END;
$function$
;

CREATE OR REPLACE FUNCTION public."IsStudent"(userid uuid)
    RETURNS boolean
    LANGUAGE plpgsql
    SECURITY DEFINER
AS
$function$
BEGIN
    RETURN EXISTS (SELECT 1
                   FROM "Students"
                   WHERE "Id" = userId);
END;
$function$
;

CREATE OR REPLACE FUNCTION public."IsTeacher"(user_id uuid)
    RETURNS boolean
    LANGUAGE sql
    SECURITY DEFINER
AS
$function$
SELECT EXISTS (SELECT 1
               FROM "Teachers"
               WHERE "Id" = user_id);
$function$
;

CREATE OR REPLACE FUNCTION public."IsTeacherInCourse"(userid uuid, code character varying, year bigint, semester bigint)
    RETURNS boolean
    LANGUAGE sql
    SECURITY DEFINER
AS
$function$
SELECT EXISTS (SELECT 1
               FROM "CourseTeachers"
               WHERE "TeacherId" = userId
                 AND "CourseCode" = code
                 AND "CourseYear" = year
                 AND "CourseSemesterCode" = semester);
$function$
;

CREATE OR REPLACE FUNCTION public.create_api_key()
    RETURNS character varying
    LANGUAGE plpgsql
AS
$function$
BEGIN
    RETURN encode(gen_random_bytes(32), 'hex') AS api_key;
END;
$function$
;

CREATE OR REPLACE FUNCTION public.hash_apikey(input_text character varying)
    RETURNS character varying
    LANGUAGE plpgsql
AS
$function$
BEGIN
    RETURN encode(digest(input_text, 'sha256'), 'base64');
END;
$function$
;

CREATE OR REPLACE FUNCTION public."isStudentInCourse"(studentid uuid, code character varying, year integer,
                                                      semester integer)
    RETURNS boolean
    LANGUAGE sql
    SECURITY DEFINER
AS
$function$
SELECT EXISTS (SELECT 1
               FROM "Enrollments"
               WHERE "StudentId" = studentId
                 AND "CourseCode" = code
                 AND "CourseYear" = year
                 AND "CourseSemesterCode" = semester);
$function$
;

CREATE UNIQUE INDEX "CourseStreaksMonthly_unique_idx" ON public."CourseStreaksMonthly" USING btree ("Id", "CourseCode", "CourseYear", "CourseSemesterCode");

create policy "Allow Authenticated"
    on "public"."Locations"
    as permissive
    for select
    to authenticated
    using (true);


create policy "read banners (auth)"
    on "public"."app_banners"
    as permissive
    for select
    to authenticated
    using (true);


create policy "Allow Teachers to view students in course"
    on "public"."Students"
    as permissive
    for select
    to authenticated
    using ("IsTeacher"(auth.uid()));