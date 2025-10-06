import {createClient} from 'npm:@supabase/supabase-js@2/';


export async function isAdmin(req: Request): Promise<boolean> {
    const authHeader = req.headers.get('Authorization')!
    const token = authHeader.replace('Bearer ', '')
    
    if (token && token === Deno.env.get('SUPABASE_SERVICE_ROLE_KEY')) {
        return true;
    }

    const supabaseClient = await getServiceRoleClient();
    const user = await supabaseClient.auth.getUser(token);
    const admin = await supabaseClient
        .from('Admins')
        .select('*')
        .eq('Id', user.data.user?.id)
        .single();
    return !!user.data.user && admin.data !== null;
}

export async function isTeacher(req: Request): Promise<boolean> {
    const authHeader = req.headers.get('Authorization')!
    const token = authHeader.replace('Bearer ', '')

    const supabaseClient = await getServiceRoleClient();
    const user = await supabaseClient.auth.getUser(token);
    const teacher = await supabaseClient
        .from('Teachers')
        .select('*')
        .eq('Id', user.data.user?.id)
        .single();
    return !!user.data.user && teacher.data !== null;
}

export async function getAnonClient() {
    const supabaseUrl = Deno.env.get('SUPABASE_URL') ?? ''
    const anonKey = Deno.env.get('SUPABASE_ANON_KEY') ?? ''
    const options = {
        auth: {
            autoRefreshToken: false,
            persistSession: false,
            detectSessionInUrl: false,
        },
    }
    return createClient(supabaseUrl, anonKey, options);
}

export async function getServiceRoleClient() {
    const supabaseUrl = Deno.env.get('SUPABASE_URL') ?? ''
    const anonKey = Deno.env.get('SUPABASE_ANON_KEY') ?? ''
    const secretKey = Deno.env.get('SUPABASE_SERVICE_ROLE_KEY') ?? ''
    const options = {
        global: { 
            headers: { 
                Authorization: `Bearer ${secretKey}` 
            } 
        },
        auth: {
            autoRefreshToken: false,
            persistSession: false,
            detectSessionInUrl: false,
        },
    }
    return createClient(supabaseUrl, secretKey, options);
}
