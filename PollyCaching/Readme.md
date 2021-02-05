###Polly Caching

Polly allows caching of responses, for reuse by a subsequent request.
Both local(mem cache), and distributed(Redis) caches are supported.

A policy can be configured in the Startup.cs file, so that the policy is applied to all requests.
The policies can be bypassed when executing, if desired.

Documentation for the package can be found at:

https://github.com/App-vNext/Polly/wiki/Cache

https://github.com/App-vNext/Polly.Caching.MemoryCache
