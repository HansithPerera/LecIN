alter table "public"."Cameras" add constraint "Cameras_Location_fkey" FOREIGN KEY ("Location") REFERENCES public."Locations"("Id") not valid;

alter table "public"."Cameras" validate constraint "Cameras_Location_fkey";


