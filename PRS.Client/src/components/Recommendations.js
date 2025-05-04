import { useEffect, useState } from "react";
import axios from "../api/axios";
import "../css/Recommendations.css";
import { useNotification } from "../contexts/NotificationContext";

function Recommendations({ context }) {
  const { notify } = useNotification();
  const [recommendations, setRecommendations] = useState([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    fetchRecommendations();
  }, [context]);

  const fetchRecommendations = () => {
    setLoading(true);

    axios.get(`/api/recommendations?context=${encodeURIComponent(context)}`)
      .then(res => {
        setRecommendations(res.data.data);
      })
      .catch(() => {
        window.location.href = "/login";
        notify("Session expired", "error");
      })
      .finally(() => setLoading(false));
  };

  if (loading) {
    return <div className="recom-loading">Loading recommendations...</div>;
  }

  if (recommendations.length === 0) {
    return null;
  }

  return (
    <div className="recom-page">
      <h2 className="recom-title">Recommended for you</h2>
      <div className="recom-product-list">
        {recommendations.map(product => (
          <div
            key={product.id}
            onClick={() => window.location.href = `/product/${product.id}`}
            className="recom-product-card"
          >
            {product.image ? (
              <div className="recom-product-card-image-wrapper">
                <img
                  src={`data:image/jpeg;base64,${product.image}`}
                  alt={product.name}
                  className="recom-product-card-image"
                />
              </div>
            ) : (
              <div className="recom-product-card-noimage">No Image</div>
            )}
            <div className="recom-product-card-name">{product.name}</div>

            <div className="recom-product-card-rating">
              <div>
                {Array.from({ length: 5 }, (_, i) => (
                  <span key={i}>
                    {i < Math.round(product.rating || 0) ? '★' : '☆'}
                  </span>
                ))}
              </div>
              <div className="recom-product-card-rating-count">
                ({product.ratingCount || 0})
              </div>
            </div>

            <div className="recom-product-card-price">
              {product.price}€
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

export default Recommendations;
