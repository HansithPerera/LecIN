// Import required libraries and modules
import {assert, assertEquals} from 'jsr:@std/assert@1'
import {createClient, SupabaseClient} from 'npm:@supabase/supabase-js@2' // Will load the .env file to Deno.env
import 'jsr:@std/dotenv/load'
import { v4 as uuidv4 } from 'uuid';
import {getAnonClient, getServiceRoleClient} from "../_shared/auth.ts";


// Set up the configuration for the Supabase client


// Test the creation and functionality of the Supabase client
const testClientCreation = async () => {
    const client: SupabaseClient = await getServiceRoleClient();
    
    const { data: newCourseData, error: newCourseError } = await client
        .from('Courses')
        .upsert({ Code: 'TEST101', SemesterCode: 1, Year: 2023, Name: 'Test Course' })

    if (newCourseError) {
        throw new Error('Failed to insert new course: ' + newCourseError.message)
    }
    
    const { data, error } = await client.auth.signUp({
        email: `${uuidv4()}@email.com`,
        password: 'example-password',
    });
    
    if (error) {
        throw new Error('Failed to sign up user: ' + error.message)
    }
    
    console.log(data);
    
    const { data: newStudentData, error: newStudentError } = await client
        .from('Students')
        .upsert({ Id: data.user?.id, FirstName: 'Test', LastName: 'Student' });
    
    if (newStudentError) {
        throw new Error('Failed to insert new student: ' + newStudentError.message)
    }
    
    const { data: newLocationData, error: newLocationError } = await client
        .from('Locations')
        .upsert({ Id: 'Room 101', Room: '101', Level: '1', Building: 'Main', Coords: 'POINT(-122.4194 37.7749)' })
    
    if (newLocationError) {
        throw new Error('Failed to insert new location: ' + newLocationError.message)
    }
    
    const { data: newClassData, error: newClassError } = await client
        .from('Classes')
        .upsert({ CourseCode: 'TEST101', CourseSemesterCode: 1, CourseYear: 2023, StartTime: new Date().toISOString(), EndTime: new Date(Date.now() + 3600000).toISOString(), Location: 'Room 101' })
        .select('Id')
        .single();
    
    if (newClassError) {
        throw new Error('Failed to insert new class: ' + newClassError.message)
    }
    
    const { data: newAttendanceData, error: newAttendanceError } = await client
        .from('Attendance')
        .upsert({ ClassId: newClassData.Id, StudentId: data.user?.id, Timestamp: new Date().toISOString() });
    
    if (newAttendanceError) {
        throw new Error('Failed to insert new attendance: ' + newAttendanceError.message)
    }
}

const testUnauthorizedWithAnon = async () => {
    const client: SupabaseClient = await getAnonClient()

    // Invoke the 'hello-world' function with a parameter
    const { error, data } = await client.functions.invoke('create-camera', {
        body: { Name: 'bar', Location: 'baz' },
    })
    
    console.log(JSON.stringify(error, null, 2))

    assertEquals(error.name, "FunctionsHttpError");
}

const testSuccessWithServiceKey = async () => {
    const client: SupabaseClient = await getServiceRoleClient();

    // Invoke the 'hello-world' function with a parameter
    const { error, data } = await client.functions.invoke('create-camera', {
        body: { Name: 'bar', Location: 'baz' },
        method: 'POST'
    })

    console.log(JSON.stringify(error, null, 2))
    
    assert(!error);
    assertEquals(data[0].Name, "bar");
    assertEquals(data[0].Location, "baz");
}

const testExportAttendanceCsv = async () => {
    const client: SupabaseClient = await getServiceRoleClient();

    const { error, data } = await client.functions.invoke('export-attendance-csv', {
        body: { Code: 'TEST101', SemesterCode: 1, Year: 2023 },
    })
    
    console.log(JSON.stringify(error, null, 2))
    console.log(JSON.stringify(data, null, 2))
    
    assert(!error);
    assert(data.csv.includes("TEST101"));
    assert(data.csv.includes("Test Course"));
    assert(data.csv.includes("Student"));
}

// Register and run the tests
Deno.test('Client Creation Test', testClientCreation)

Deno.test('Export Attendance CSV Test',{ sanitizeResources: false }, testExportAttendanceCsv)
Deno.test('Unauthorized with Anon Key Test',{ sanitizeResources: false }, testUnauthorizedWithAnon)
Deno.test('Success with Service Key Test',{ sanitizeResources: false }, testSuccessWithServiceKey)