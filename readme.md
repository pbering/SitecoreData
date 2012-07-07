# SitecoreData

Experiments with [Sitecore](http://www.sitecore.net) running on [RavenDB](http://ravendb.net) and [MongoDB](http://www.mongodb.org). Primary goals:

* Universal DataProvider wrapper for simplifying writing data providers for Sitecore.
* Read only data provider for serialized Sitecore items, intented for quick Sitecore prototyping and testing (or think TDS).
* Fully production ready data providers, including support for event queue, history, blob storage, the full monty!
* "Event Sourcing" stylede data providers (basically all CRUD operations is only inserts which enables some 
interesting data replication benefits).

## News ##

# Publishing to a mongo target database now works from a shell running on SQL master and core!
# Transfer.aspx now transfers all versions.
# The workflow workbox now works in a shell running on mongodb master and core.

## Performance ##

See [/doc/Measurements.xlsx](https://github.com/pbering/SitecoreData/blob/master/doc/Measurements.xlsx) for current performance data.

## Notes ##

Inspiration for the MongoDB implementation came from this blog post: (http://hermanussen.eu/sitecore/wordpress/2012/05/making-sitecore-faster-with-mongodb/), big thanks 
to Robin Hermanussen for getting me inspired to trying out MongoDB!

This is tested with Sitecore 6.4.1 rev. 110324 and Sitecore 6.5.0 rev. 120427.

## Installation ##

* Clone
* Place Sitecore.Kernel.dll in \lib\Sitecore\
* Configure (connection strings, data folder etc. setup IIS website with clean Sitecore installation) and build
* Deploy (Use build configuration "Debug with Publish" and a deploycommand.xml pointing to above IIS website)
* Run /Transfer.aspx

## Contributing ##

Please do :)