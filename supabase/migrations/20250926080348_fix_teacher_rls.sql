create policy "Allow Teachers in course to view"
on "public"."Attendance"
as permissive
for select
to authenticated
using ((EXISTS ( SELECT 1
   FROM "Classes" c
  WHERE ((c."Id" = "Attendance"."ClassId") AND "IsTeacherInCourse"(auth.uid(), c."CourseCode", ((c."CourseYear")::integer)::bigint, ((c."CourseSemesterCode")::integer)::bigint)))));


create policy "Allow Teachers to view students in course"
on "public"."Students"
as permissive
for select
to public
using ("IsTeacher"(auth.uid()));



