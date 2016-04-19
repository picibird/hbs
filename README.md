# Hybrid Bookshelf #

This is the opensource version of the hybrid bookshelf. A product version with support is offered by [picibird](http://www.picibird.com "picibird"). 

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
