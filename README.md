# Boxofon

## TODO

- Verify private phone number using Twilio's "OutgoingCallerId" resource.
  Does not carry a cost and has the additional benefit of allowing the
  private phone number to be used as a caller id when making outgoing calls
  (if that would ever be needed).

- Use the per-number authKey when authenticating requests from Twilio.

- Use async in modules that perform long-running tasks (such as calls to Twilio).

- Split the solution into two web apps; one for the public management portal
  and one for the Twilio app.