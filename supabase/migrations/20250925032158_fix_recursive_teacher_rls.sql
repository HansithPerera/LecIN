drop policy "Allow Teachers to view other teachers" on "public"."Teachers";

set check_function_bodies = off;

CREATE OR REPLACE FUNCTION public."IsTeacher"(user_id uuid)
 RETURNS boolean
 LANGUAGE sql
 SECURITY DEFINER
AS $function$SELECT EXISTS (
        SELECT 1
        FROM "Teachers"
        WHERE "Id" = user_id
    );$function$
;

CREATE OR REPLACE FUNCTION public."IsAdmin"(userid uuid)
 RETURNS boolean
 LANGUAGE plpgsql
 SECURITY DEFINER
AS $function$BEGIN
  RETURN EXISTS (
      SELECT 1
      FROM "Admins"
      WHERE "Id" = userId
  );
END;$function$
;

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

create policy "Allow Teachers to view other teachers"
on "public"."Teachers"
as permissive
for select
to authenticated
using ("IsTeacher"(auth.uid()));



