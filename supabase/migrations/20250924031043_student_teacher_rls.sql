drop policy "Enable users to view their own data only" on "public"."Teachers";

drop policy "Allow admins to view all other admins" on "public"."Admins";

set check_function_bodies = off;

CREATE OR REPLACE FUNCTION public."IsStudent"(userid uuid)
 RETURNS boolean
 LANGUAGE plpgsql
 SECURITY DEFINER
AS $function$BEGIN
  RETURN EXISTS (
      SELECT 1
      FROM "Students"
      WHERE "Id" = userId
  );
END;$function$
;

CREATE OR REPLACE FUNCTION public."IsTeacherInCourse"(userid uuid, code character varying, year bigint, semester bigint)
 RETURNS boolean
 LANGUAGE sql
 SECURITY DEFINER
AS $function$SELECT EXISTS (
        SELECT 1
        FROM "CourseTeachers"
        WHERE "TeacherId" = userId
          AND "CourseCode" = code
          AND "CourseYear" = year
          AND "CourseSemesterCode" = semester
    );$function$
;

CREATE OR REPLACE FUNCTION public."AdminHasPermission"(userid uuid, permname character varying)
 RETURNS boolean
 LANGUAGE plpgsql
 SECURITY DEFINER
AS $function$DECLARE
    perms int;
    permValue int;
BEGIN
    SELECT "Permissions" INTO perms
    FROM "Admins"
    WHERE "Id" = userId;

    IF NOT FOUND THEN
        RETURN FALSE;
    END IF;

    SELECT "Value" INTO permValue
    FROM "PermissionFlags"
    WHERE "Name" ILIKE permName;

    RETURN (perms & permValue) = permValue;
END;$function$
;

create policy "Allow Admins with ReadApiKeys to view"
on "public"."ApiKeys"
as permissive
for select
to authenticated
using ("AdminHasPermission"(auth.uid(), 'readapikeys'::character varying));


create policy "Allow Students to view own"
on "public"."Attendance"
as permissive
for select
to authenticated
using ((( SELECT auth.uid() AS uid) = "StudentId"));


create policy "Allow Admins with ReadApiKeys and ReadCameras"
on "public"."CameraApiKeys"
as permissive
for select
to authenticated
using (("AdminHasPermission"(auth.uid(), 'readapikeys'::character varying) AND "AdminHasPermission"(auth.uid(), 'readcameras'::character varying)));


create policy "Allow Admins with ReadCameras to read cameras"
on "public"."Cameras"
as permissive
for select
to public
using ("AdminHasPermission"(auth.uid(), 'readcameras'::character varying));


create policy "Allow Course Teachers within a course"
on "public"."Classes"
as permissive
for select
to authenticated
using ((EXISTS ( SELECT 1
   FROM "CourseTeachers" ct
  WHERE ((ct."TeacherId" = auth.uid()) AND (((ct."CourseCode")::text = ("Classes"."CourseCode")::text) AND (ct."CourseYear" = "Classes"."CourseYear") AND (ct."CourseSemesterCode" = "Classes"."CourseSemesterCode"))))));


create policy "Allow Students to view enrolled classes"
on "public"."Classes"
as permissive
for select
to authenticated
using ((EXISTS ( SELECT 1
   FROM "Enrollments" e
  WHERE ((e."StudentId" = auth.uid()) AND ((e."CourseCode")::text = (e."CourseCode")::text) AND (e."CourseYear" = e."CourseYear") AND (e."CourseSemesterCode" = e."CourseSemesterCode")))));


create policy "Allow  teachers in same course"
on "public"."CourseTeachers"
as permissive
for select
to authenticated
using ("IsTeacherInCourse"(auth.uid(), "CourseCode", "CourseYear", "CourseSemesterCode"));


create policy "Allow Admins with ReadTeachers and ReadCourses"
on "public"."CourseTeachers"
as permissive
for select
to authenticated
using (("AdminHasPermission"(auth.uid(), 'readteachers'::character varying) AND "AdminHasPermission"(auth.uid(), 'readcourses'::character varying)));


create policy "Allow CourseTeachers to view own Courses"
on "public"."Courses"
as permissive
for select
to authenticated
using ((EXISTS ( SELECT 1
   FROM "CourseTeachers" ct
  WHERE ((ct."TeacherId" = auth.uid()) AND ((ct."CourseCode")::text = ("Courses"."Code")::text) AND (ct."CourseYear" = "Courses"."Year") AND (ct."CourseSemesterCode" = "Courses"."SemesterCode")))));


create policy "Allow Students To View Enrolled Courses"
on "public"."Courses"
as permissive
for select
to authenticated
using ((EXISTS ( SELECT 1
   FROM "Enrollments" e
  WHERE ((e."StudentId" = auth.uid()) AND ((e."CourseCode")::text = ("Courses"."Code")::text) AND (e."CourseYear" = "Courses"."Year") AND (e."CourseSemesterCode" = "Courses"."SemesterCode")))));


create policy "Allow Teachers to view enrolled in course"
on "public"."Enrollments"
as permissive
for select
to authenticated
using ((EXISTS ( SELECT 1
   FROM "CourseTeachers" ct
  WHERE ((ct."TeacherId" = auth.uid()) AND ((ct."CourseCode")::text = ("Enrollments"."CourseCode")::text) AND (ct."CourseYear" = "Enrollments"."CourseYear") AND (ct."CourseSemesterCode" = "Enrollments"."CourseSemesterCode")))));


create policy "Allow Admins to view"
on "public"."PermissionFlags"
as permissive
for select
to authenticated
using ("IsAdmin"(auth.uid()));


create policy "Allow Student to view own face"
on "public"."StudentFaces"
as permissive
for select
to authenticated
using ((( SELECT auth.uid() AS uid) = "StudentId"));


create policy "Enable Teachers to view their own data only"
on "public"."Teachers"
as permissive
for select
to authenticated
using ((( SELECT auth.uid() AS uid) = "Id"));


create policy "Allow admins to view all other admins"
on "public"."Admins"
as permissive
for select
to authenticated
using ("IsAdmin"(auth.uid()));



