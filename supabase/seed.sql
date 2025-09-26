INSERT INTO auth.users (instance_id, id, aud, role, email, encrypted_password,
                        email_confirmed_at, recovery_sent_at, last_sign_in_at,
                        raw_app_meta_data, raw_user_meta_data,
                        created_at, updated_at,
                        confirmation_token, email_change, email_change_token_new, recovery_token)
VALUES ('00000000-0000-0000-0000-000000000000', '11111111-1111-1111-1111-111111111111', 'authenticated',
        'authenticated', 'user1@example.com', crypt('password123', gen_salt('bf')), now(), now(), now(),
        '{"provider":"email","providers":["email"]}', '{}'::jsonb, now(), now(), '', '', '', ''),
       ('00000000-0000-0000-0000-000000000000', '22222222-2222-2222-2222-222222222222', 'authenticated',
        'authenticated', 'user2@example.com', crypt('password123', gen_salt('bf')), now(), now(), now(),
        '{"provider":"email","providers":["email"]}', '{}'::jsonb, now(), now(), '', '', '', ''),
       ('00000000-0000-0000-0000-000000000000', '33333333-3333-3333-3333-333333333333', 'authenticated',
        'authenticated', 'user3@example.com', crypt('password123', gen_salt('bf')), now(), now(), now(),
        '{"provider":"email","providers":["email"]}', '{}'::jsonb, now(), now(), '', '', '', ''),
       ('00000000-0000-0000-0000-000000000000', '44444444-4444-4444-4444-444444444444', 'authenticated',
        'authenticated', 'user4@example.com', crypt('password123', gen_salt('bf')), now(), now(), now(),
        '{"provider":"email","providers":["email"]}', '{}'::jsonb, now(), now(), '', '', '', ''),
       ('00000000-0000-0000-0000-000000000000', '55555555-5555-5555-5555-555555555555', 'authenticated',
        'authenticated', 'user5@example.com', crypt('password123', gen_salt('bf')), now(), now(), now(),
        '{"provider":"email","providers":["email"]}', '{}'::jsonb, now(), now(), '', '', '', ''),
       ('00000000-0000-0000-0000-000000000000', '66666666-6666-6666-6666-666666666666', 'authenticated',
        'authenticated', 'user6@example.com', crypt('password123', gen_salt('bf')), now(), now(), now(),
        '{"provider":"email","providers":["email"]}', '{}'::jsonb, now(), now(), '', '', '', ''),
       ('00000000-0000-0000-0000-000000000000', '77777777-7777-7777-7777-777777777777', 'authenticated',
        'authenticated', 'user7@example.com', crypt('password123', gen_salt('bf')), now(), now(), now(),
        '{"provider":"email","providers":["email"]}', '{}'::jsonb, now(), now(), '', '', '', ''),
       ('00000000-0000-0000-0000-000000000000', '88888888-8888-8888-8888-888888888888', 'authenticated',
        'authenticated', 'user8@example.com', crypt('password123', gen_salt('bf')), now(), now(), now(),
        '{"provider":"email","providers":["email"]}', '{}'::jsonb, now(), now(), '', '', '', ''),
       ('00000000-0000-0000-0000-000000000000', '99999999-9999-9999-9999-999999999999', 'authenticated',
        'authenticated', 'user9@example.com', crypt('password123', gen_salt('bf')), now(), now(), now(),
        '{"provider":"email","providers":["email"]}', '{}'::jsonb, now(), now(), '', '', '', ''),
       ('00000000-0000-0000-0000-000000000000', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'authenticated',
        'authenticated', 'user10@example.com', crypt('password123', gen_salt('bf')), now(), now(), now(),
        '{"provider":"email","providers":["email"]}', '{}'::jsonb, now(), now(), '', '', '', '');

INSERT INTO auth.identities (id, user_id, provider_id, identity_data, provider, last_sign_in_at, created_at, updated_at)
VALUES (uuid_generate_v4(), '11111111-1111-1111-1111-111111111111', '11111111-1111-1111-1111-111111111111',
        '{"sub":"11111111-1111-1111-1111-111111111111","email":"user1@example.com"}', 'email', now(), now(), now()),
       (uuid_generate_v4(), '22222222-2222-2222-2222-222222222222', '22222222-2222-2222-2222-222222222222',
        '{"sub":"22222222-2222-2222-2222-222222222222","email":"user2@example.com"}', 'email', now(), now(), now()),
       (uuid_generate_v4(), '33333333-3333-3333-3333-333333333333', '33333333-3333-3333-3333-333333333333',
        '{"sub":"33333333-3333-3333-3333-333333333333","email":"user3@example.com"}', 'email', now(), now(), now()),
       (uuid_generate_v4(), '44444444-4444-4444-4444-444444444444', '44444444-4444-4444-4444-444444444444',
        '{"sub":"44444444-4444-4444-4444-444444444444","email":"user4@example.com"}', 'email', now(), now(), now()),
       (uuid_generate_v4(), '55555555-5555-5555-5555-555555555555', '55555555-5555-5555-5555-555555555555',
        '{"sub":"55555555-5555-5555-5555-555555555555","email":"user5@example.com"}', 'email', now(), now(), now()),
       (uuid_generate_v4(), '66666666-6666-6666-6666-666666666666', '66666666-6666-6666-6666-666666666666',
        '{"sub":"66666666-6666-6666-6666-666666666666","email":"user6@example.com"}', 'email', now(), now(), now()),
       (uuid_generate_v4(), '77777777-7777-7777-7777-777777777777', '77777777-7777-7777-7777-777777777777',
        '{"sub":"77777777-7777-7777-7777-777777777777","email":"user7@example.com"}', 'email', now(), now(), now()),
       (uuid_generate_v4(), '88888888-8888-8888-8888-888888888888', '88888888-8888-8888-8888-888888888888',
        '{"sub":"88888888-8888-8888-8888-888888888888","email":"user8@example.com"}', 'email', now(), now(), now()),
       (uuid_generate_v4(), '99999999-9999-9999-9999-999999999999', '99999999-9999-9999-9999-999999999999',
        '{"sub":"99999999-9999-9999-9999-999999999999","email":"user9@example.com"}', 'email', now(), now(), now()),
       (uuid_generate_v4(), 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
        '{"sub":"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa","email":"user10@example.com"}', 'email', now(), now(), now());

INSERT INTO "public"."PermissionFlags" ("Id", "Name", "BitPosition")
VALUES ('1', 'ReadTeachers', '0'),
       ('2', 'ReadStudents', '1'),
       ('3', 'ReadCourses', '2'),
       ('4', 'ReadReports', '3'),
       ('5', 'ReadApiKeys', '4'),
       ('6', 'ReadCameras', '5'),
       ('7', 'WriteTeachers', '6'),
       ('8', 'WriteStudents', '7'),
       ('9', 'WriteCourses', '8'),
       ('10', 'WriteReports', '9'),
       ('11', 'WriteApiKeys', '10'),
       ('12', 'WriteCameras', '11'),
       ('13', 'ExportData', '12');

INSERT INTO "Teachers" ("Id", "FirstName", "LastName", "CreatedAt", "UpdatedAt")
VALUES ('11111111-1111-1111-1111-111111111111', 'Amelia', 'Clark', now(), NULL),
       ('22222222-2222-2222-2222-222222222222', 'Benjamin', 'Lee', now(), NULL),
       ('33333333-3333-3333-3333-333333333333', 'Charlotte', 'Harris', now(), NULL),
       ('44444444-4444-4444-4444-444444444444', 'Daniel', 'Walker', now(), NULL),
       ('55555555-5555-5555-5555-555555555555', 'Ella', 'Robinson', now(), NULL);

INSERT INTO "Courses" ("Code", "Name", "Year", "SemesterCode")
VALUES ('CS101', 'Introduction to Computer Science', 2023, 1),
       ('MATH201', 'Calculus I', 2023, 1),
       ('PHY301', 'Physics I', 2023, 2),
       ('ENG101', 'English Literature', 2023, 2),
       ('HIST201', 'World History', 2023, 1);

INSERT INTO "CourseTeachers" ("TeacherId", "CourseCode", "CourseSemesterCode", "CourseYear")
VALUES ('11111111-1111-1111-1111-111111111111', 'CS101', 1, 2023),
       ('22222222-2222-2222-2222-222222222222', 'MATH201', 1, 2023),
       ('33333333-3333-3333-3333-333333333333', 'PHY301', 2, 2023),
       ('44444444-4444-4444-4444-444444444444', 'ENG101', 2, 2023),
       ('55555555-5555-5555-5555-555555555555', 'HIST201', 1, 2023),
       ('11111111-1111-1111-1111-111111111111', 'MATH201', 1, 2023);

INSERT INTO "Classes" ("Id", "CourseCode", "CourseYear", "CourseSemesterCode", "StartTime", "EndTime", "Location")
VALUES (gen_random_uuid(), 'CS101', 2023, 1, '2023-09-01 09:00:00+00', '2023-09-01 10:30:00+00', 'Room 101'),
       (gen_random_uuid(), 'MATH201', 2023, 1, '2023-09-01 11:00:00+00', '2023-09-01 12:30:00+00', 'Room 102'),
       (gen_random_uuid(), 'PHY301', 2023, 2, '2023-09-02 09:00:00+00', '2023-09-02 10:30:00+00', 'Room 201'),
       (gen_random_uuid(), 'ENG101', 2023, 2, '2023-09-02 11:00:00+00', '2023-09-02 12:30:00+00', 'Room 202'),
       (gen_random_uuid(), 'HIST201', 2023, 1, '2023-09-03 09:00:00+00', '2023-09-03 10:30:00+00', 'Room 301'),
       (gen_random_uuid(), 'MATH201', 2023, 1, '2023-09-03 11:00:00+00', '2023-09-03 12:30:00+00', 'Room 102'),
       (gen_random_uuid(), 'CS101', 2023, 1, '2023-09-04 09:00:00+00', '2023-09-04 10:30:00+00', 'Room 101'),
       (gen_random_uuid(), 'PHY301', 2023, 2, '2023-09-04 11:00:00+00', '2023-09-04 12:30:00+00', 'Room 201'),
       (gen_random_uuid(), 'ENG101', 2023, 2, '2023-09-05 09:00:00+00', '2023-09-05 10:30:00+00', 'Room 202'),
       (gen_random_uuid(), 'HIST201', 2023, 1, '2023-09-05 11:00:00+00', '2023-09-05 12:30:00+00', 'Room 301');

INSERT INTO "Students" ("Id", "FirstName", "LastName", "CreatedAt", "UpdatedAt")
VALUES ('22222222-2222-2222-2222-222222222222', 'Liam', 'Johnson', now(), NULL),
       ('33333333-3333-3333-3333-333333333333', 'Olivia', 'Smith', now(), NULL),
       ('44444444-4444-4444-4444-444444444444', 'Noah', 'Williams', now(), NULL),
       ('55555555-5555-5555-5555-555555555555', 'Ava', 'Brown', now(), NULL),
       ('66666666-6666-6666-6666-666666666666', 'Elijah', 'Jones', now(), NULL),
       ('77777777-7777-7777-7777-777777777777', 'Sophia', 'Garcia', now(), NULL),
       ('88888888-8888-8888-8888-888888888888', 'James', 'Miller', now(), NULL),
       ('99999999-9999-9999-9999-999999999999', 'Isabella', 'Davis', now(), NULL),
       ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'William', 'Rodriguez', now(), NULL);   

INSERT INTO "Attendance" ("ClassId", "StudentId", "Timestamp", "Reason")
VALUES
((SELECT "Id" FROM "Classes" WHERE "CourseCode" = 'CS101' AND "StartTime" = '2023-09-01 09:00:00+00'), '22222222-2222-2222-2222-222222222222', '2023-09-01 09:05:00+00', NULL),
((SELECT "Id" FROM "Classes" WHERE "CourseCode" = 'CS101' AND "StartTime" = '2023-09-01 09:00:00+00'), '33333333-3333-3333-3333-333333333333', '2023-09-01 09:10:00+00', NULL),
((SELECT "Id" FROM "Classes" WHERE "CourseCode" = 'MATH201' AND "StartTime" = '2023-09-01 11:00:00+00'), '44444444-4444-4444-4444-444444444444', '2023-09-01 11:05:00+00', NULL),
((SELECT "Id" FROM "Classes" WHERE "CourseCode" = 'MATH201' AND "StartTime" = '2023-09-01 11:00:00+00'), '55555555-5555-5555-5555-555555555555', '2023-09-01 11:15:00+00', NULL),
((SELECT "Id" FROM "Classes" WHERE "CourseCode" = 'PHY301' AND "StartTime" = '2023-09-02 09:00:00+00'), '66666666-6666-6666-6666-666666666666', '2023-09-02 09:05:00+00', NULL),
((SELECT "Id" FROM "Classes" WHERE "CourseCode" = 'PHY301' AND "StartTime" = '2023-09-02 09:00:00+00'), '77777777-7777-7777-7777-777777777777', '2023-09-02 09:20:00+00', NULL),
((SELECT "Id" FROM "Classes" WHERE "CourseCode" = 'ENG101' AND "StartTime" = '2023-09-02 11:00:00+00'), '88888888-8888-8888-8888-888888888888', '2023-09-02 11:05:00+00', NULL),
((SELECT "Id" FROM "Classes" WHERE "CourseCode" = 'HIST201' AND "StartTime" = '2023-09-03 09:00:00+00'), 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', '2023-09-03 09:05:00+00', NULL),
((SELECT "Id" FROM "Classes" WHERE "CourseCode" = 'HIST201' AND "StartTime" = '2023-09-03 09:00:00+00'), '22222222-2222-2222-2222-222222222222', '2023-09-03 09:25:00+00', NULL),
((SELECT "Id" FROM "Classes" WHERE "CourseCode" = 'MATH201' AND "StartTime" = '2023-09-03 11:00:00+00'), '33333333-3333-3333-3333-333333333333', '2023-09-03 11:05:00+00', NULL),
((SELECT "Id" FROM "Classes" WHERE "CourseCode" = 'MATH201' AND "StartTime" = '2023-09-03 11:00:00+00'), '44444444-4444-4444-4444-444444444444', '2023-09-03 11:15:00+00', NULL),
((SELECT "Id" FROM "Classes" WHERE "CourseCode" = 'CS101' AND "StartTime" = '2023-09-04 09:00:00+00'), '55555555-5555-5555-5555-555555555555', '2023-09-04 09:05:00+00', NULL),
((SELECT "Id" FROM "Classes" WHERE "CourseCode" = 'CS101' AND "StartTime" = '2023-09-04 09:00:00+00'), '66666666-6666-6666-6666-666666666666', '2023-09-04 09:30:00+00', NULL),
((SELECT "Id" FROM "Classes" WHERE "CourseCode" = 'PHY301' AND "StartTime" = '2023-09-04 11:00:00+00'), '77777777-7777-7777-7777-777777777777', '2023-09-04 11:05:00+00', NULL),
((SELECT "Id" FROM "Classes" WHERE "CourseCode" = 'PHY301' AND "StartTime" = '2023-09-04 11:00:00+00'), '88888888-8888-8888-8888-888888888888', '2023-09-04 11:15:00+00', NULL),
((SELECT "Id" FROM "Classes" WHERE "CourseCode" = 'ENG101' AND "StartTime" = '2023-09-05 09:00:00+00'), 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', '2023-09-05 09:20:00+00', NULL),
((SELECT "Id" FROM "Classes" WHERE "CourseCode" = 'HIST201' AND "StartTime" = '2023-09-05 11:00:00+00'), '22222222-2222-2222-2222-222222222222', '2023-09-05 11:05:00+00', NULL),
((SELECT "Id" FROM "Classes" WHERE "CourseCode" = 'HIST201' AND "StartTime" = '2023-09-05 11:00:00+00'), '33333333-3333-3333-3333-333333333333', '2023-09-05 11:15:00+00', NULL);