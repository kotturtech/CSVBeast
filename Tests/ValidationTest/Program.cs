﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using KotturTech.CSVBeast;
using KotturTech.CSVBeast.CSVDataBuilder;
using KotturTech.CSVBeast.Customization.Implementations;
using KotturTech.CSVBeast.Errata;

namespace ValidationTest
{
    namespace BasicComponentTest
    {
        class ArtistInfo
        {
            [CSVExport("Name", SortOrder = 5)]
            public string Name { get; set; }

            [StringLength(5)]
            [CSVExport("LastName", SortOrder = 3)]
            public string LastName { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as ArtistInfo;
                if (other == null)
                    return false;

                return Name.Equals(other.Name) && LastName.Equals(other.LastName);
            }
        }

        class GuitarData
        {
            [CSVExport("Id", SortOrder = 1)]
            public int Id { get; set; }

            [CSVExport("Manufacturer", SortOrder = 2)]
            public string Manufacturer { get; set; }

            [CSVExport("Model", SortOrder = 3)]
            public string Model { get; set; }

            [Range(1000,2000)]
            [CSVExport("Year", SortOrder = 4)]
            public int Year { get; set; }

            [CSVExport("Artists", SortOrder = 5, CustomExporterType = typeof(ComponentListCSVExporter))]
            public List<ArtistInfo> UsedByArtists { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as GuitarData;
                if (other == null)
                    return false;

                bool res = Id.Equals(other.Id) && Manufacturer.Equals(other.Manufacturer) && Model.Equals(other.Model) &&
                       Year.Equals(other.Year);
                if (!res)
                    return false;

                if (UsedByArtists == null)
                {
                    if (other.UsedByArtists != null)
                        return false;
                    return true;
                }

                if (other.UsedByArtists == null)
                    return false;

                if (UsedByArtists.Count != other.UsedByArtists.Count)
                    return false;

                for (int i = 0; i < UsedByArtists.Count; i++)
                {
                    if (!UsedByArtists[i].Equals(other.UsedByArtists[i]))
                        return false;
                }
                return true;

            }
        }

        class Program
        {
            private GuitarData[] _dataSet;

            private Dictionary<string, string[]> _manufacturersToModels = new Dictionary<string, string[]>
        {
            {"Jackson", new[] {"Randy Rhoads", "Soloist", "Kelly", "Warrior", "V"}},
            {"BC Rich", new[] {"Mockingbird", "Ironbird", "Warlock", "Stealth", "Ignitor"}},
            {"Gibson", new[] {"Explorer", "Flying V", "SG", "Les Paul", "Firebird"}},
            {"ESP", new[] {"Viper", "Horizon", "The Mirage", "Eclipse", "KX"}},
            {"Ibanez", new[] {"RG", "S", "Xiphos", "Iceman", "JEM"}}
        };

            private void GenerateData(int dataSetSize)
            {
                var rand = new Random();
                var keys = _manufacturersToModels.Keys.ToArray();
                _dataSet = new GuitarData[dataSetSize];
                var names = new[] { "Randy", "Glenn", "Jeff", "Richie", "Tony" };
                var lastnames = new[] { "Rhoads", "Iommi", "Tipton", "Blackmore", "Hanneman" };
                for (int i = 0; i < dataSetSize; i++)
                {
                    var manufacturer = keys[rand.Next(0, 4)];
                    var model = _manufacturersToModels[manufacturer][rand.Next(0, 4)];
                    var artistList = new List<ArtistInfo>();
                    var quant = rand.Next(0, 5);
                    for (var j = 0; j < quant; j++)
                    {
                        artistList.Add(new ArtistInfo
                        {
                            Name = names[rand.Next(0, 5)],
                            LastName = lastnames[rand.Next(0, 5)]
                        });
                    }
                    var gtr = new GuitarData
                    {
                        Id = i,
                        Manufacturer = manufacturer,
                        Model = model,
                        Year = rand.Next(1980, 2015),
                        UsedByArtists = artistList
                    };

                    _dataSet[i] = gtr;
                }
            }

            static void Main(string[] args)
            {
                var p = new Program();
                int dataSetSize = 10;
                Console.WriteLine("Generating dataset with {0} items....", dataSetSize);
                p.GenerateData(dataSetSize);
                Console.WriteLine("Done!!!");
                CSVExporter exp = new CSVExporter("Data.csv");
                exp.ProgressChanged += exp_ProgressChanged;
                exp.ErrorOccurred += exp_ErrorOccurred;
                exp.ExportData(p._dataSet);
                Console.WriteLine("Completed!!! Now will import back....");
                Console.ReadKey();


                List<GuitarData> data = new List<GuitarData>();
                CSVImporter importer = new CSVImporter("Data.csv");
                importer.ProgressChanged += importer_ProgressChanged;
                importer.ErrorOccurred += importer_ErrorOccurred;
                importer.ImportToDataSet(data);
                Console.WriteLine("Import Completed! {0} rows imported. Ready to test!", data.Count);
                Console.ReadKey();
                var errs = 0;
                //for (var i = 0; i < dataSetSize; i++)
                //{
                //    if (!p._dataSet[i].Equals(data[i]))
                //    {
                //        Console.WriteLine("Mismatch on line {0}", i);
                //        errs++;
                //    }
                //}

                //if (errs == 0)
                //    Console.WriteLine("Perfect Match!");
                //Console.ReadKey();

                Console.WriteLine("Exporting data for comparision...");
                exp = new CSVExporter("Data2.csv");
                exp.ProgressChanged += exp_ProgressChanged;
                exp.ErrorOccurred += exp_ErrorOccurred;
                exp.ExportData(data);
                Console.WriteLine("Completed!!!");

                Console.ReadKey();


            }

            static void importer_ErrorOccurred(object sender, CSVImportExportErrorEventArgs e)
            {
                Console.WriteLine("Import Error: {0}", e.Exception != null ? e.Exception.Message : "Unknown");
            }

            static void importer_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
            {
                Console.WriteLine("{0}% completed", e.ProgressPercentage);
            }

            static void exp_ErrorOccurred(object sender, CSVImportExportErrorEventArgs e)
            {
                Console.WriteLine("Export Error: {0}", e.Exception != null ? e.Exception.Message : "Unknown");
            }

            static void exp_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
            {
                Console.WriteLine("{0}% completed", e.ProgressPercentage);
            }
        }
    }

}
