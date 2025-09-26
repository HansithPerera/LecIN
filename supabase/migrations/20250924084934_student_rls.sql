create extension if not exists "pgtap" with schema "extensions";


drop policy "Allow Admins with ReadCameras to read cameras" on "public"."Cameras";

drop policy "Allow Students to view enrolled classes" on "public"."Classes";

drop policy "Allow Students To View Enrolled Courses" on "public"."Courses";

drop policy "Allow Teachers to view enrolled in course" on "public"."Enrollments";

alter table "public"."Attendance" add column "Reason" text;

drop extension if exists "pgtap";

set check_function_bodies = off;

CREATE OR REPLACE FUNCTION public."isStudentInCourse"(studentid uuid, code character varying, year integer, semester integer)
 RETURNS boolean
 LANGUAGE sql
 SECURITY DEFINER
AS $function$SELECT EXISTS (
        SELECT 1
        FROM "Enrollments"
        WHERE "StudentId" = studentId
          AND "CourseCode" = code
          AND "CourseYear" = year
          AND "CourseSemesterCode" = semester
    );$function$
;

create policy "Allow Students to view own enrollments"
on "public"."Enrollments"
as permissive
for select
to public
using (("StudentId" = auth.uid()));


create policy "Allow Teachers to view other teachers"
on "public"."Teachers"
as permissive
for select
to authenticated
using ((EXISTS ( SELECT 1
   FROM "Teachers" "Teachers_1"
  WHERE ("Teachers_1"."Id" = auth.uid()))));


create policy "Allow Admins with ReadCameras to read cameras"
on "public"."Cameras"
as permissive
for select
to authenticated
using ("AdminHasPermission"(auth.uid(), 'readcameras'::character varying));


create policy "Allow Students to view enrolled classes"
on "public"."Classes"
as permissive
for select
to authenticated
using ("isStudentInCourse"(auth.uid(), "CourseCode", ("CourseYear")::integer, ("CourseSemesterCode")::integer));


create policy "Allow Students To View Enrolled Courses"
on "public"."Courses"
as permissive
for select
to authenticated
using ("isStudentInCourse"(auth.uid(), "Code", ("Year")::integer, ("SemesterCode")::integer));


create policy "Allow Teachers to view enrolled in course"
on "public"."Enrollments"
as permissive
for select
to authenticated
using ("IsTeacherInCourse"(auth.uid(), "CourseCode", "CourseYear", "CourseSemesterCode"));



