// Credential Management API type declarations
// https://developer.mozilla.org/en-US/docs/Web/API/Credential_Management_API

interface PasswordCredentialData {
    id: string;
    password: string;
    name?: string;
    iconURL?: string;
}

declare class PasswordCredential extends Credential {
    constructor(data: PasswordCredentialData);
    readonly password: string;
    readonly name: string;
    readonly iconURL: string;
}

interface Window {
    PasswordCredential: typeof PasswordCredential;
}

interface CredentialsContainer {
    store(credential: Credential): Promise<Credential>;
}
