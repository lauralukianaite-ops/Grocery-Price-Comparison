export function getDiscountPercentage(offer) {
  const hasDiscount = offer?.retailCost > 0 && offer.retailCost > offer.cost;
  return hasDiscount ? ((offer.retailCost - offer.cost) / offer.retailCost) * 100 : 0;
}

export function getProductDiscountPercentage(product) {
  return Math.max(0, ...product.offers.map(getDiscountPercentage));
}

export function getBestOffer(offers) {
  return offers.reduce((bestOffer, offer) => {
    if (!bestOffer || offer.cost < bestOffer.cost) return offer;
    if (offer.cost === bestOffer.cost && offer.storeId < bestOffer.storeId) return offer;
    return bestOffer;
  }, null);
}

function compareByName(left, right) {
  return left.name.localeCompare(right.name) || String(left.id).localeCompare(String(right.id));
}

export function compareCatalogProducts(left, right, sort) {
  if (sort === "name") return compareByName(left, right);
  if (sort === "name-desc") {
    return right.name.localeCompare(left.name) || String(left.id).localeCompare(String(right.id));
  }

  const leftPrice = left.bestOffer?.cost;
  const rightPrice = right.bestOffer?.cost;
  if (leftPrice == null || rightPrice == null) {
    return Number(leftPrice == null) - Number(rightPrice == null) || compareByName(left, right);
  }

  const priceDifference = leftPrice - rightPrice;
  if (sort === "price-low") return priceDifference || compareByName(left, right);
  if (sort === "price-high") return -priceDifference || compareByName(left, right);

  const discountDifference =
    getProductDiscountPercentage(left) - getProductDiscountPercentage(right);
  const discountOrder = sort === "discount-low" ? discountDifference : -discountDifference;
  return discountOrder || priceDifference || compareByName(left, right);
}
