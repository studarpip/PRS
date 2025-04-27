import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import axios from "axios";
import { useCart } from "../contexts/CartContext";
import "../css/Product.css";

function Product() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { fetchCartCount } = useCart();

  const [product, setProduct] = useState(null);
  const [loading, setLoading] = useState(true);
  const [cartCount, setCartCount] = useState(1);
  const [cartItem, setCartItem] = useState(null);
  const [feedbackMessage, setFeedbackMessage] = useState("");

  const [userCanRate, setUserCanRate] = useState(false);
  const [userRating, setUserRating] = useState(0);

  useEffect(() => {
    fetchProduct();
    fetchCartItem();
    fetchCanRate();
  }, [id]);

  const fetchProduct = () => {
    axios.get(`/api/products/${id}`)
      .then(res => setProduct(res.data.data))
      .catch(() => {
        alert("Failed to load product");
        navigate("/home");
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
          setCartCount(found.count);
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
            setUserRating(data.previousRating);
          }
        }
      })
      .catch(() => { });
  };

  const handleUpdateCart = () => {
    if (cartItem && cartItem.count === cartCount) {
      setFeedbackMessage("No changes to update.");
      setTimeout(() => setFeedbackMessage(""), 2000);
      return;
    }

    axios.post("/api/cart", {
      productId: product.id,
      count: cartCount
    })
      .then(() => {
        fetchCartCount();
        fetchCartItem();
        setFeedbackMessage("Cart updated!");
        setTimeout(() => setFeedbackMessage(""), 2000);
      })
      .catch(() => {
        alert("Failed to update cart");
      });
  };

  const handleSubmitRating = () => {
    if (userRating < 1 || userRating > 5) {
      alert("Please select a rating between 1 and 5.");
      return;
    }

    axios.post("/api/rating", {
      productId: product.id,
      rating: userRating
    })
      .then(() => {
        alert("Rating submitted!");
        fetchProduct();
        fetchCanRate();
      })
      .catch(() => {
        alert("Failed to submit rating.");
      });
  };

  if (loading) return <p>Loading product...</p>;
  if (!product) return <p>Product not found.</p>;

  return (
    <div className="product-page">
      <button onClick={() => navigate(-1)} className="product-back-button">← Back</button>

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
            <div>
              {Array.from({ length: 5 }, (_, i) => (
                <span key={i}>
                  {i < Math.round(product.rating || 0) ? '★' : '☆'}
                </span>
              ))}
            </div>
            <div className="product-rating-count">({product.ratingCount || 0})</div>
          </div>

          <div className="product-price">{product.price}€</div>

          <div className="product-description">
            {product.description || "No description available."}
          </div>

          {cartItem && (
            <div className="product-cart-in-cart">In your cart</div>
          )}

          <div className="product-quantity-selector">
            <label>Quantity:</label>
            <button onClick={() => setCartCount(prev => Math.max(1, prev - 1))}>-</button>
            <span>{cartCount}</span>
            <button onClick={() => setCartCount(prev => prev + 1)}>+</button>
          </div>

          <button
            onClick={handleUpdateCart}
            className="product-add-to-cart-button"
          >
            {cartItem ? "Update Cart" : "Add to Cart"}
          </button>

          {feedbackMessage && (
            <div className="product-feedback-message">
              {feedbackMessage}
            </div>
          )}

          {userCanRate && (
            <div className="product-rating-section">
              <h3>Rate this Product</h3>
              <div className="product-rating-stars">
                {Array.from({ length: 5 }, (_, i) => (
                  <span
                    key={i}
                    className={i < userRating ? "selected-star" : ""}
                    onClick={() => setUserRating(i + 1)}
                  >
                    ★
                  </span>

                ))}
              </div>
              <button
                onClick={handleSubmitRating}
                className="product-rate-button"
              >
                {userRating ? "Update Rating" : "Submit Rating"}
              </button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default Product;
