// HbsPortableAssembly.cs
// Date Created: 28.01.2016
// 
// Copyright (c) 2016, picibird GmbH 
// All rights reserved.
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using picibird.hbs;
using picibird.hbs.cover;
using picibird.hbs.ldu;
using picibits.bib;
using picibits.core;
using SimpleInjector;

[assembly: PiciAssemblyExport(typeof(HbsPortableAssemebly))]

namespace picibird.hbs
{
    public class HbsPortableAssemebly : IPiciAssembly
    {
        public void OnLoad()
        {
        }

        public void Inject(PiciInjections injections)
        {
            injections.Container.Register<ICoverService, Covers>(Lifestyle.Singleton);

            injections.Container.Register<IHitCoverLoader, HitCoverLoader>(Lifestyle.Singleton);
        }
    }
}