export function getCookie(name: string): string | null {
    const match = document.cookie.match(new RegExp(`(^| )${name}=([^;]+)`));
    return match ? decodeURIComponent(match[2]) : null;
}

export function setCookie(name: string, value: string, days: number = 28, path: string = "/"): void {
    const expires = `${new Date(Date.now() + days * 86400000).toUTCString()}`
    document.cookie = `${name}=${encodeURIComponent(value)};expires=${expires};path=${path}`;
}