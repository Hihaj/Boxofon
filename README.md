# Boxofon

## TODO

- Implement support for receiving sms.

- Implement support for sending sms.

- Implement some form of authentication of incoming e-mails. How can we know
  that the sender is not spoofed? SPF + DKIM?

- Automatically import Twilio numbers upon authorization (to allow for re-signups).

- Use async in modules that perform long-running tasks (such as calls to Twilio).

- Split the solution into two web apps; one for the public management portal
  and one for the Twilio app.

- Create a design for the management portal web app.

- Create a favicon-friendly logo.