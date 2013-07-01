using System;
using System.IO;
using System.Xml;

namespace Geocode
{
  public class NLGeocode
	{
		
		public static void Main()
		{
			string[] addressString;
			string postalCode = GetPostalCode().ToUpper();
			string buildingNumber = GetBuildingNumber();
			string searchString = "http://geodata.nationaalgeoregister.nl/geocoder/Geocoder?zoekterm=" + postalCode + "+" + buildingNumber;
			string fileName = "Adressen.csv"; //Name output file
			
			addressString = NLGeocoder(searchString);

			for (int y = 0; y < addressString.Length; y++)
			{
				Console.WriteLine();
				Console.WriteLine(addressString[y]);
			}
			if (addressString[0] == "Geen resultaat gevonden")
			{
				return;
			}
			if (!File.Exists(fileName))
			{
				using (StreamWriter file = new StreamWriter(fileName, true))
				{
					file.WriteLine("Straat, Huisnummer, Postcode, Plaats, Gemeente, Provincie, X, Y"); //Header line output file
				}
			}
			using (StreamWriter file = new StreamWriter(fileName, true))
			{
				for (int y = 0; y < addressString.Length; y++)
				{
					file.WriteLine(addressString[y]);
				}
			}
			Console.WriteLine();
			FileInfo info = new FileInfo(fileName);
			Console.WriteLine("Het resultaat is opgeslagen in het bestand {0}", info.FullName);
		}

		public static string[] NLGeocoder(string searchString)
		{
			string[] resultString = {"Geen resultaat gevonden"};
			string street, buildingNumber, postalCode, town, municipality, province, xCoordinate, yCoordinate;
			int x, y;
			x = -1;
			
			XmlReader reader = XmlReader.Create(searchString);
			
			while (reader.Read())
			{
				if (reader.NodeType == XmlNodeType.Element && reader.Name == "xls:GeocodeResponseList")
				{
					y = Convert.ToInt32(reader.GetAttribute(0));
					Array.Resize(ref resultString, y);
				}
				if (reader.NodeType == XmlNodeType.Element && reader.Name == "xls:GeocodedAddress")
				{
					x++;
					while (reader.NodeType != XmlNodeType.EndElement)
					{
						reader.Read();
						if (reader.Name == "gml:Point")
						{
							XmlReader inner = reader.ReadSubtree();
							inner.ReadToDescendant("gml:pos");
							inner.Read();
							string[] coords = inner.Value.Split(new char[] { ' ' });
							xCoordinate = coords[0];
							yCoordinate = coords[1];
							resultString[x] = xCoordinate + "," + yCoordinate;
							inner.Close();
							reader.Read();
						}
						if (reader.Name == "xls:Address")
						{
							XmlReader inner = reader.ReadSubtree();
							inner.ReadToFollowing("xls:Building");
							buildingNumber = "";
							if (inner.HasAttributes)
							{
								while (reader.MoveToNextAttribute())
								{
									if (buildingNumber == "")
									{
										buildingNumber = inner.Value;
									}
									else
									{
										buildingNumber = buildingNumber + " " + inner.Value;
									}
								}
								// Move the reader back to the element node.
								reader.MoveToElement();
							}
							inner.ReadToFollowing("xls:Street");
							inner.Read();
							street = inner.Value;
							inner.ReadToFollowing("xls:Place"); //MunicipalitySubdivision
							inner.Read();
							town = inner.Value;
							inner.ReadToFollowing("xls:Place"); //Municipality
							inner.Read();
							municipality = inner.Value;
							inner.ReadToFollowing("xls:Place"); //CountrySubdivision
							inner.Read();
							province = inner.Value;
							inner.ReadToFollowing("xls:PostalCode");
							inner.Read();
							postalCode = inner.Value;
							inner.Close();
							resultString[x] = street + "," + buildingNumber + "," + postalCode + "," + town + "," + municipality + "," + province + "," + resultString[x];
							reader.Read();
						}
					}
				}
			}
			reader.Close();
			return resultString;
		}

		public static string GetPostalCode()
		{
			String postalCode = "";
			do
			{
				Console.Write("Geef postcode (formaat 1234AB): "); //Please enter a Dutch postal code (format 1234AB): 
				postalCode = Console.ReadLine();
			} while (!ValidatePostalCode(postalCode));
			return postalCode;
		}

		public static bool ValidatePostalCode(string postalCode)
		{
			int x;
			x = 0;
			if (string.IsNullOrEmpty(postalCode) || postalCode.Length != 6)
			{
				return false;
			}

			foreach (char item in postalCode)
			{
				x++;			
				if (x <= 4 && !char.IsDigit(item))
				{
					return false;
				}
				else if (x > 4 && !char.IsLetter(item))
				{
					return false;
				}
			}

			return true;
		}

		public static string GetBuildingNumber()
		{
			String buildingNumber = "";
			do
			{
				Console.Write("Geef huisnummer (evt. met toevoeging): "); //Please enter a building number (optionally including suffix):
				buildingNumber = Console.ReadLine();
			} while (!ValidateBuildingNumber(buildingNumber));
			return buildingNumber;
		}

		public static bool ValidateBuildingNumber(string buildingNumber)
		{
			int x;
			x = 0;
			if (string.IsNullOrEmpty(buildingNumber) || buildingNumber.Length > 15)
			{
				return false;
			}

			foreach (char item in buildingNumber)
			{
				x++;			
				if (x <= 1 && !char.IsDigit(item))
				{
					return false;
				}
			}

			return true;
		}
	}
}
