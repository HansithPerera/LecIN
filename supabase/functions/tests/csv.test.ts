// Import required libraries and modules
import { assert, assertEquals } from 'jsr:@std/assert@1'
import { createClient, SupabaseClient } from 'npm:@supabase/supabase-js@2'// Will load the .env file to Deno.env

import 'jsr:@std/dotenv/load'

// Set up the configuration for the Supabase client
const supabaseUrl = Deno.env.get('SUPABASE_URL') ?? ''
const supabaseKey = Deno.env.get('SUPABASE_PUBLISHABLE_KEY') ?? ''
const serviceKey = Deno.env.get('SUPABASE_SERVICE_ROLE_KEY') ?? ''
const options = {
    auth: {
        autoRefreshToken: false,
        persistSession: false,
        detectSessionInUrl: false,
    },
}

// Test the creation and functionality of the Supabase client
const testClientCreation = async () => {
    const client: SupabaseClient = createClient(supabaseUrl, serviceKey, options)
    if (!supabaseUrl) throw new Error('supabaseUrl is required.')
    if (!supabaseKey) throw new Error('supabaseKey is required.')

    const { data: table_data, error: table_error } = await client
        .from('ApiKeys')
        .select('*')
        .limit(1)
    if (table_error) {
        throw new Error('Invalid Supabase client: ' + table_error.message)
    }
    console.log(JSON.stringify(table_data, null, 2))
    assert(table_data, 'Data should be returned from the query.')
}

const testUnauthorizedWithAnon = async () => {
    const client: SupabaseClient = createClient(supabaseUrl, supabaseKey, options)

    // Invoke the 'hello-world' function with a parameter
    const { error, data } = await client.functions.invoke('create-camera', {
        body: { Name: 'bar', Location: 'baz' },
    })
    
    console.log(JSON.stringify(error, null, 2))

    assertEquals(error.name, "FunctionsHttpError");
}

const testSuccessWithServiceKey = async () => {
    const client: SupabaseClient = createClient(supabaseUrl, serviceKey, options)

    // Invoke the 'hello-world' function with a parameter
    const { error, data } = await client.functions.invoke('create-camera', {
        body: { Name: 'bar', Location: 'baz' },
    })

    console.log(JSON.stringify(data, null, 2))

    assertEquals(data[0].Name, "bar");
    assertEquals(data[0].Location, "baz");
}

// Register and run the tests
Deno.test('Client Creation Test', testClientCreation)
Deno.test('Unauthorized with Anon Key Test',{ sanitizeResources: false }, testUnauthorizedWithAnon)
Deno.test('Success with Service Key Test',{ sanitizeResources: false }, testSuccessWithServiceKey)