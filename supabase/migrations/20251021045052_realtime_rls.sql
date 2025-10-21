create policy "Authenticated users can receive broadcasts"
    on "realtime"."messages"
    for select
    to authenticated
    using ( true );