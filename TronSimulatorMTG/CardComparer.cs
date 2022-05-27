using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TronSimulatorMTG.CardInfo;

namespace TronSimulatorMTG
{
	class CardComparer : IEqualityComparer<Card>
	{
		// Products are equal if their names and product numbers are equal.
		public bool Equals(Card x, Card y)
		{

			//Check whether the compared objects reference the same data.
			if (Object.ReferenceEquals(x, y)) return true;

			//Check whether any of the compared objects is null.
			if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
				return false;

			//Check whether the products' properties are equal.
			return x.Name == y.Name;
		}

		// If Equals() returns true for a pair of objects
		// then GetHashCode() must return the same value for these objects.

		public int GetHashCode(Card card)
		{
			//Check whether the object is null
			if (Object.ReferenceEquals(card, null)) return 0;

			//Get hash code for the Name field if it is not null.
			int hashCardName = card.Name == null ? 0 : card.Name.GetHashCode();

			//Get hash code for the Code field.
			//int hashProductCode = card.Code.GetHashCode();

			//Calculate the hash code for the product.
			return hashCardName;
		}
	}
}
