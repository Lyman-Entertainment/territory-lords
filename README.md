# territory-lords
Code base for Territory Lords Online Game

To run this locally you'll need some AzureAdB2C setting set somewhere, not in appsettings.json or anywhere that gets checked in. Currently this app does not use Azure App Config or keyvault so it's all done in ENV vars in the app itself.

A template appsettings.Development.json would loook like this but with values from the B2C tenant.
```
{
  "AzureAdB2C": {
    "Instance": "https://lamegameentertainment.b2clogin.com",
    "CallbackPath": "/signin-oidc",
    "Domain": "lamegameentertainment.onmicrosoft.com",
    "SignedOutCallbackPath": "/signout",
    "SignUpSignInPolicyId": "B2C_1_Signup_Signin",
    "ResetPasswordPolicyId": "B2C_1_PasswordReset",
    "EditProfilePolicyId": "",
    "ClientId": "",
    "ClientSecret": ""
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information",
      "Microsoft.AspNetCore.SignalR": "Debug",
      "Microsoft.AspNetCore.Http.Connections": "Debug"
    }
  },
  "AllowedHosts": "*"
}
```
