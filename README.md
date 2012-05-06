NancySelfHost
=============

Testing project for NancyFx self-hosting

Running
-------
Before running this on Vista or Win7 please be sure you're running as Administrator or enable current user to bind port 8090 using command
`netsh http add urlacl url=http://localhost:8090/ user=<user> listen=yes delegate=yes`
where `<user>` is current user name. Use `DOMAIN\user` notation for domain users.

After running executable point your browser to http://localhost:8090/

Diagnostics page (http://localhost:8090/_Nancy) password is `123`.
