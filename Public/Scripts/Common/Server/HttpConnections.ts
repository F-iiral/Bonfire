function Connect<TBody, TResponse>(url: string, body: TBody, method: string): Promise<TResponse> | null {
    fetch(url, { method: method, body: JSON.stringify(body) })
        .then(response => {
            if (!response.ok)
                return null;
            return response.json() as Promise<TResponse>
        })
        .catch(error => {})
    return null;
}

export function Get<TResponse>(url: string): Promise<TResponse> | null {
    return Connect<null, TResponse>(url, null, "GET");
}
export function Query<TResponse, TBody>(url: string, body: TBody): Promise<TResponse> | null {
    return Connect<TBody, TResponse>(url, body, "POST");    // This is a method that doesn't exist but will hopefully be added 
}
export function Post<TResponse, TBody>(url: string, body: TBody): Promise<TResponse> | null {
    return Connect<TBody, TResponse>(url, body, "POST");
}
export function Patch<TResponse, TBody>(url: string, body: TBody): Promise<TResponse> | null {
    return Connect<TBody, TResponse>(url, body, "PATCH");
}
export function Put<TResponse, TBody>(url: string, body: TBody): Promise<TResponse> | null {
    return Connect<TBody, TResponse>(url, body, "PUT");
    }
export function Delete<TResponse, TBody>(url: string, body: TBody): Promise<TResponse> | null {
    return Connect<TBody, TResponse>(url, body, "DELETE");
}