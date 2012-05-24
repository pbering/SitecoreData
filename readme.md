# SitecoreData

Experiments with [Sitecore](http://www.sitecore.net) running on [RavenDB](http://ravendb.net) and [MongoDB](http://www.mongodb.org). Primary goals:

* DataProvider wrapper for simplifying writing data providers for Sitecore.
* Read only data provider for serialized Sitecore items, intented for quick Sitecore prototyping and testing.
* Fully production ready data providers, including support for event queue, history, blob storage, the full monty!
* "Event Sourcing" stylede data providers for (basically all CRUD operations is only inserts which enables some 
interesting data replication benefits).

## Notes ##

Inspiration for the MongoDB implementation came from this blog post: (http://hermanussen.eu/sitecore/wordpress/2012/05/making-sitecore-faster-with-mongodb/), big thanks 
to Robin Hermanussen for getting me started at trying out MongoDB!

## Installation ##

Clone, build, configure and run /Transfer*.aspx... for now, NuGet packages will come at some point! 

## Contributing ##

Please do :)