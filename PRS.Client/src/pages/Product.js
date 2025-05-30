import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import axios from "axios";
import { useCart } from "../contexts/CartContext";
import { ArrowLeft } from "lucide-react";
import { useNotification } from "../contexts/NotificationContext";
import Recommendations from "../components/Recommendations";
import "../css/Product.css";

const CategoryEnum = {
  0: "Electronics",
  1: "Gaming",
  2: "Computers",
  3: "Books",
  4: "Fiction",
  5: "Sport",
  6: "Basketball",
  7: "Clothing",
  8: "Shoes",
  9: "Music",
  10: "Instruments",
  11: "Home",
  12: "Kitchen",
  13: "Health",
  14: "Beauty",
  15: "Toys",
  16: "Pets",
  17: "Office",
  18: "Accessories",
  19: "Photography",
  20: "Outdoors"
};


function Product() {
  const { notify } = useNotification();
  const { id } = useParams();
  const navigate = useNavigate();
  const { fetchCartCount } = useCart();

  const [product, setProduct] = useState(null);
  const [loading, setLoading] = useState(true);
  const [cartCount, setCartCount] = useState(1);
  const [cartItem, setCartItem] = useState(null);

  const [userCanRate, setUserCanRate] = useState(false);
  const [previousRating, setPreviousRating] = useState(null);
  const [currentRating, setCurrentRating] = useState(0);

  useEffect(() => {
    fetchProduct();
    fetchCartItem();
    fetchCanRate();
  }, [id]);

  const fetchProduct = () => {
    axios.get(`/api/products/${id}`)
      .then(res => setProduct(res.data.data))
      .catch(() => {
        window.location.href = "/login";
        notify("Session expired", "error");
      })
      .finally(() => setLoading(false));
  };

  const fetchCartItem = () => {
    axios.get("/api/cart")
      .then(res => {
        const cart = res.data.data;
        const found = cart.find(item => item.productId === id);
        if (found) {
          setCartItem(found);
        }
      })
      .catch(() => { });
  };

  const fetchCanRate = () => {
    axios.get(`/api/rating/canRate/${id}`)
      .then(res => {
        const data = res.data.data;
        if (data) {
          setUserCanRate(data.canRate);
          if (data.previousRating !== null) {
            setPreviousRating(data.previousRating);
            setCurrentRating(0);
          }
        }
      })
      .catch(() => { });
  };

  const handleAddToCart = () => {
    const newCount = (cartItem?.count || 0) + cartCount;

    axios.post("/api/cart", {
      productId: product.id,
      count: newCount
    })
      .then(() => {
        fetchCartCount();
      })
      .catch(() => {
        window.location.href = "/login";
        notify("Session expired", "error");
      });
  };

  const handleSubmitRating = () => {
    if (currentRating < 1 || currentRating > 5) {
      notify("Please select a rating between 1 and 5.", "error");
      return;
    }

    axios.post("/api/rating", {
      productId: product.id,
      rating: currentRating
    })
      .then(() => {
        notify("Rating submitted!", "success");
        fetchProduct();
        fetchCanRate();
      })
      .catch(() => {
        window.location.href = "/login";
        notify("Session expired", "error");
      });
  };

  if (loading) {
    return <div className="prod-loading">Loading product...</div>;
  }
  if (!product) {
    return <div className="prod-loading">Product not found.</div>;
  }

  return (
    <div className="product-page">
      <button onClick={() => navigate(-1)} className="product-back-button" title="Back">
        <ArrowLeft size={20} />
      </button>

      <div className="product-content">
        <div className="product-image-container">
          {product.image ? (
            <img
              src={`data:image/jpeg;base64,${product.image}`}
              alt={product.name}
              className="product-image"
            />
          ) : (
            <div className="no-product-image">No Image</div>
          )}
        </div>

        <div className="product-details">
          <h1>{product.name}</h1>

          <div className="product-rating">
            {Array.from({ length: 5 }, (_, i) => (
              <span key={i}>
                {i < Math.round(product.rating || 0) ? '★' : '☆'}
              </span>
            ))}
            <div className="product-rating-count">({product.ratingCount || 0})</div>
          </div>

          <div className="product-price">{product.price}€</div>

          <div className="product-description">
            {product.description || "No description available."}
          </div>
          <div className="product-categories">
            <strong>Categories: </strong>
            {product.categories && product.categories.length > 0 ? (
              product.categories.map((cat, index) => (
                <span key={index}>
                  {CategoryEnum[cat]}
                  {index !== product.categories.length - 1 && ', '}
                </span>
              ))
            ) : (
              <span>No categories</span>
            )}
          </div>

        </div>

        <div className="product-side-card">
          <div className="product-side-price">{product.price}€</div>

          <div className="product-quantity-selector">
            <label>Quantity:</label>
            <button onClick={() => setCartCount(prev => Math.max(1, prev - 1))}>-</button>
            <span>{cartCount}</span>
            <button onClick={() => setCartCount(prev => prev + 1)}>+</button>
          </div>

          <button
            onClick={handleAddToCart}
            className="product-add-to-cart-button"
          >
            Add to Cart
          </button>

          {userCanRate && (
            <div className="product-rating-section">
              <h3>Rate this product</h3>
              <div className="product-rating-stars">
                {Array.from({ length: 5 }, (_, i) => (
                  <span
                    key={i}
                    className={
                      (currentRating !== 0 ? (i < currentRating) : (i < previousRating))
                        ? "selected-star"
                        : ""
                    }
                    onClick={() => setCurrentRating(i + 1)}
                  >
                    ★
                  </span>
                ))}
              </div>

              <button
                onClick={handleSubmitRating}
                className="product-rate-button"
                disabled={currentRating === 0 || currentRating === previousRating}
              >
                {previousRating !== null ? "Update Rating" : "Submit Rating"}
              </button>
            </div>
          )}
        </div>
      </div>
      <Recommendations context="product" />
    </div>
  );
}

export default Product;
