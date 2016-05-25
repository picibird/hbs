# Hybrid Bookshelf #

This is the opensource version of the hybrid bookshelf. A [commercial version](http://www.hybridbookshelf.de/#welcome) with additional features and support is offered by [picibird](http://www.picibird.com). 

The project started off with the master thesis [Blended Shelf](http://zenodo.org/record/17947#.V0QpQZGLRaQ) of [Eike Kleinert](http://www.eikekleiner.de/) at the [human computer interaction department](http://hci.uni-konstanz.de/) at [the university of konstanz](https://www.uni-konstanz.de/). The thesis evaluates a 3D bookshelf metaphor to visualize and interact with books and ebooks in a natural and appealing way and is part of the research project [blended library](http://hci.uni-konstanz.de/index.php?a=research&b=projects&c=8609071). Convinced by the prorotype's potential the [Kommunikations-, Informations-, Medienzentrum (KIM)](https://www.kim.uni-konstanz.de/) library service of the university of konstanz started the hybrid bookshelf project funded by the [MWK Baden-Württemberg](https://mwk.baden-wuerttemberg.de/de/ministerium/). The   software startup [picibird](http://www.picibird.com/) which spezialized in HCI is developing the Hybrid Bookshelf and services around it.

## Library Data Unifier ##

[LDU github repo](https://github.com/BSZBW/ldu)

This repository includes the project hbs.ldu, a portable client implementation to connect to the Library Data Unifier. 
The LDU is a customized opensource [pazpar2](http://www.indexdata.com/pazpar2) server by [BSZ] (https://www.bsz-bw.de/index.html) library services.

To get access to demo data please contact:
Clemens Elmlinger, BSZ
clemens.elmlinger@bsz-bw.de

## For Early Adopters ##

The hybrid bookshelf opensource code will have major changes and cleanup in 2016. Early adopters should create their own branch.

## Solution ##

* hbs - portable application base library
* hbs.wpf - wpf library containing basic abstract application, styles, etc.
* hbs.wpf.demo - wpf application implementation launching hbs with ldu opensource server configruation
* hbs.ldu - portable library to communicate with library data unifier (pazpar2 server)
* picibits.bib - portable base library

## Nuget Server

We host our own [nuget server](http://nuget.picibits.com/api/v2/) to distribute our picibits app development library. We aim to go opensource with picibits in 2017. This project's src folder includes a **nuget.config** file so visual studio should automatically pick up our nuget server. If not apply http://nuget.picibits.com/api/v2/ manually.

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
