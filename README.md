# Hybrid Bookshelf #

This is the open source version of the hybrid bookshelf. A [commercial version](http://www.hybridbookshelf.de/#welcome) with additional features and support is offered by [picibird](http://www.picibird.com). 

The project started off with the master thesis [Blended Shelf](http://zenodo.org/record/17947#.V0QpQZGLRaQ) of [Eike Kleinert](http://www.eikekleiner.de/) at the [human-computer interaction department](http://hci.uni-konstanz.de/) at [the University of Konstanz](https://www.uni-konstanz.de/). The thesis evaluates a 3D bookshelf metaphor to visualize and interact with books and ebooks in a natural and appealing way and is part of the research project [Blended Library](http://hci.uni-konstanz.de/index.php?a=research&b=projects&c=8609071). Convinced by the prototype's potential the [Kommunikations-, Informations-, Medienzentrum (KIM)](https://www.kim.uni-konstanz.de/) library service of the university of Konstanz started the hybrid bookshelf project funded by the [MWK Baden-Württemberg](https://mwk.baden-wuerttemberg.de/de/ministerium/). The   software startup [picibird](http://www.picibird.com/) which specialized in HCI won the contract and is developing the Hybrid Bookshelf and services around it.

## Open Source Version##

The open source version contains the basic .Net WPF application and works with the [Library Data Unifier LDU](https://github.com/BSZBW/ldu), a [pazpar2](http://www.indexdata.com/pazpar2) server maintained by the [BSZ] (https://www.bsz-bw.de/index.html).

To get access to demo data please contact: Clemens Elmlinger, BSZ - clemens.elmlinger@bsz-bw.de

##  Commercial Version ##

The commercial version builds upon the open source version and offers extended functionality and web services around it like:
* connect to other backends than LDU like Solr or EBSCO
* colorize the 3D books based on the book cover's primary colors using our cover image analyzing service
* take media information like availability or location on your smartphone using qr-codesn and our webservice [bibshelf](http://kn.bibshelf.de/#/dashboard)
* collect media and maintain lists on your smartphone or tablet on our webservice [bibshelf](http://kn.bibshelf.de/#/dashboard)
* share books and other media directly from the Hybrid Bookshelf via email
* a setup.exe with your configuration preloaded for easy installation and start up

To get more information or request a demo please contact: mail@picibird.com

## Visual Studio Solution ##

* hbs - portable application base library
* hbs.wpf - wpf library containing basic abstract application, styles, etc.
* hbs.wpf.demo - wpf application implementation launching hbs with ldu opensource server configruation
* hbs.ldu - portable library to communicate with library data unifier (pazpar2 server)
* picibits.bib - portable base library

## Nuget Server

We host our own [nuget server](http://nuget.picibits.com/api/v2/) to distribute our picibits app development library. We aim to go open source with picibits in 2017. This project's src folder includes a **nuget.config** file so visual studio should automatically pick up our nuget server. If not apply http://nuget.picibits.com/api/v2/ manually.

## Build ##

* visual studio 2015 (uses .net 4.6 portable class libraries)
* nuget 3.0 or later
* restore nuget packages if not automatically
* set **hbs.wpf.demo** as startup project

## License ##

Copyright (c) 2016 picibird GmbH
All rights reserved.

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.

## Who do I talk to? ##

* mail@picibird.com
