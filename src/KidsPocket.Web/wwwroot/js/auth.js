function waitFor(checkReady, timeoutMs) {
    return new Promise((resolve, reject) => {
        if (checkReady()) return resolve();
        const start = Date.now();
        const interval = setInterval(() => {
            if (checkReady()) {
                clearInterval(interval);
                resolve();
            } else if (Date.now() - start > timeoutMs) {
                clearInterval(interval);
                reject(new Error("הסקריפט לא נטען בזמן"));
            }
        }, 50);
    });
}

window.kidsPocketAuth = {
    // הסקריפט של Google (accounts.google.com/gsi/client) נטען עם async/defer, אז הוא לרוב עוד
    // לא מוכן כשה-circuit של Blazor מגיע ל-OnAfterRenderAsync הראשון - מחכים לו בפולינג במקום
    // לבדוק פעם אחת ולוותר בשקט אם הוא עוד לא שם.
    renderGoogleButton: async function (clientId, dotNetRef, elementId) {
        try {
            await waitFor(() => !!window.google?.accounts?.id, 8000);
        } catch {
            console.error("Google Identity Services לא נטען בזמן");
            return;
        }
        google.accounts.id.initialize({
            client_id: clientId,
            callback: (response) => dotNetRef.invokeMethodAsync("OnGoogleCredential", response.credential)
        });
        google.accounts.id.renderButton(document.getElementById(elementId), {
            type: "standard", theme: "outline", size: "large", text: "continue_with", shape: "pill", width: 300
        });
    },

    signInWithApple: async function (clientId, redirectUri) {
        try {
            await waitFor(() => !!window.AppleID, 8000);
        } catch {
            throw new Error("Sign in with Apple JS לא נטען בזמן");
        }
        AppleID.auth.init({
            clientId: clientId,
            scope: "name email",
            redirectURI: redirectUri,
            usePopup: true
        });
        const response = await AppleID.auth.signIn();
        const user = response.user;
        return {
            idToken: response.authorization.id_token,
            email: user?.email ?? null,
            displayName: user?.name ? `${user.name.firstName} ${user.name.lastName}`.trim() : null
        };
    }
};
