# Boxofon

## TODO

- Verify private phone numbers using either the user's own Twilio number or
  Twilio's "OutgoingCallerId" resource. Both of these options would incur no
  cost to Boxofon when verifying numbers.

- Use the per-number authKey when authenticating requests from Twilio.

- Use async in modules that perform long-running tasks (such as calls to Twilio).

- Split the solution into two web apps; one for the public management portal
  and one for the Twilio app.

- Create a design for the management portal web app.

- Create a favicon-friendly logo.