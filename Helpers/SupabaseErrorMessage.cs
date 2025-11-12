namespace SnapLabel.Helpers {
    public static class SupabaseErrorMessage {

        public static string GetErrorMessage(string errorCode) {

            return errorCode switch {

                "weak_password" => "Password should be at least 6 characters.",
                "user_already_registered" => "This email is already registered.",
                "validation_failed" => "The email format is invalid. Please enter a valid email like 'name@example.com'.",
                "email_address_invalid" => "The email you entered is invalid, please enter a new one.",
                "rate_limit_exceeded" => "Too many requests. Please try again later.",
                "invalid_login_credentials" => "Incorrect email or password.",
                "email_not_confirmed" => "Please confirm your email before logging in.",
                "user_not_found" => "No account found with this email.",
                "invalid_credentials" => "Incorrect email or password.",
                "email_signup_disabled" => "Email signups are currently disabled.",
                "provider_not_allowed" => "This authentication provider is not allowed.",
                _ => "An unexpected error occurred."
            };
        }
    }
}
