// Pazpar2.cs
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

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using picibits.core.mvvm;

namespace picibird.hbs.ldu
{
    [XmlRoot("init")]
    public class PazPar2Init : Model
    {
        [XmlElement("status")]
        public string Status { get; set; }

        [XmlElement("session")]
        public int Session { get; set; }
    }


    [XmlRoot("stat")]
    public class PazPar2Stat : Model
    {
        //<stat>
        //    <activeclients>1</activeclients>
        //    <hits>0</hits>
        //    <records>0</records>
        //    <clients>1</clients>
        //    <unconnected>0</unconnected>
        //    <connecting>0</connecting>
        //    <working>1</working>
        //    <idle>0</idle>
        //    <failed>0</failed>
        //    <error>0</error>
        //    <progress>0.00</progress>
        //</stat>


        [XmlElement("hits")]
        public int hits { get; set; }

        [XmlElement("records")]
        public int records { get; set; }

        [XmlElement("progress")]
        public double progress { get; set; }

        public int progressAsInt()
        {
            return int.Parse((progress*100).ToString());
        }
    }


    //<show>
    //  <status>OK</status>
    //  <activeclients>12</activeclients>
    //  <merged>62</merged>
    //  <total>462</total>
    //  <start>0</start>
    //  <num>20</num>
    //</show>

    [XmlRoot("show")]
    public class PazPar2Show : Model
    {
        [XmlElement("num")]
        public int num { get; set; }

        [XmlElement("merged")]
        public int merged { get; set; }

        [XmlElement("total")]
        public int total { get; set; }

        [XmlElement("start")]
        public int start { get; set; }


        [XmlElement("hit")]
        public List<Hit> Hits { get; set; }

        public override string ToString()
        {
            return String.Format("num={0}; merged={1}; total={2}; start={3}; hitCount={4};", num, merged, total, start,
                Hits.Count);
        }
    }


    //<termlist>
    //<activeclients>0</activeclients>
    //<list name=""language"">
    //<term><name>English</name><frequency>68</frequency></term>
    //<term><name>eng</name><frequency>62</frequency></term>
    //<term><name>ger</name><frequency>45</frequency></term>
    //<term><name>French</name><frequency>12</frequency></term>
    //<term><name>German</name><frequency>7</frequency></term>
    //<term><name>Spanish</name><frequency>4</frequency></term>
    //<term><name>Swedish</name><frequency>4</frequency></term>
    //<term><name>fre</name><frequency>2</frequency></term>
    //<term><name>Dutch</name><frequency>1</frequency></term>
    //<term><name>Portuguese</name><frequency>1</frequency></term>
    //</list>
    //<list name=""author"">
    //<term><name>Einstein, Albert</name><frequency>6</frequency></term>
    //<term><name>Clark, Ronald William</name><frequency>3</frequency></term>
    //<term><name>Leggett, Anthony J.</name><frequency>3</frequency></term>
    //<term><name>Balibar, Franc¸oise</name><frequency>2</frequency></term>
    //<term><name>Broglie, Louis de</name><frequency>2</frequency></term>
    //<term><name>Clark, Ronald W</name><frequency>2</frequency></term>
    //<term><name>Dongen, Jeroen van</name><frequency>2</frequency></term>
    //<term><name>Frank, Philipp</name><frequency>2</frequency></term>
    //<term><name>Frank, Philipp G</name><frequency>2</frequency></term>
    //<term><name>Girbau, Joan</name><frequency>2</frequency></term>
    //<term><name>Kennedy, Robert E.</name><frequency>2</frequency></term>
    //<term><name>Merleau-Ponty, Jacques</name><frequency>2</frequency></term>
    //<term><name>Moszkowski, Alexander</name><frequency>2</frequency></term>
    //<term><name>Mo¨hrke, Philipp Bernd</name><frequency>2</frequency></term>
    //<term><name>ABRAHAM, Pierre</name><frequency>1</frequency></term>
    //</list>
    //[..]
    //<list name=""xtargets"">
    //<term>
    //<id>eiger.bsz-bw.de:8000/summon</id>
    //<name>TestSummon</name>
    //<frequency>1215472</frequency>
    //<state>Client_Idle</state>
    //<diagnostic>0</diagnostic>
    //</term>
    //<term>
    //<id>z3950n.bsz-bw.de:20210/swb367</id>
    //<name>SWB Lokale Sicht UB Konstanz</name>
    //<frequency>623</frequency>
    //<state>Client_Idle</state>
    //<diagnostic>0</diagnostic>
    //</term>
    //</list>
    //</termlist>
    [XmlRoot("termlist")]
    public class Pazpar2Termlist : Model
    {
        [XmlElement("activeclients")]
        public int activeclients { get; set; }

        [XmlElement("list")]
        public List<FilterCategory> filterCategories { get; set; }
    }
}