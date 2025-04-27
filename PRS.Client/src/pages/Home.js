import { useEffect, useState } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import axios from "axios";
import ProductFilters from "../components/ProductFilters";
import "../css/Home.css";

function Home() {
  const navigate = useNavigate();
  const location = useLocation();

  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(false);

  const [categories, setCategories] = useState([]);
  const [orderByOptions, setOrderByOptions] = useState([]);

  const [filters, setFilters] = useState({
    input: "",
    selectedCategories: [],
    priceFrom: "",
    priceTo: "",
    ratingFrom: "",
    ratingTo: "",
    orderBy: "",
  });

  const [page, setPage] = useState(1);
  const pageSize = 20;
  const [hasMore, setHasMore] = useState(false);
  const [searched, setSearched] = useState(false);

  useEffect(() => {
    fetchFilters();
  }, []);

  useEffect(() => {
    const params = new URLSearchParams(location.search);

    const restoredFilters = {
      input: params.get("input") || "",
      selectedCategories: params.getAll("categories").map(Number),
      priceFrom: params.get("priceFrom") || "",
      priceTo: params.get("priceTo") || "",
      ratingFrom: params.get("ratingFrom") || "",
      ratingTo: params.get("ratingTo") || "",
      orderBy: params.get("orderBy") || "",
    };

    const restoredPage = parseInt(params.get("page") || "1", 10);

    setFilters(restoredFilters);
    setPage(restoredPage);

    if (params.toString()) {
      setSearched(true);
      fetchProducts(restoredFilters, restoredPage);
    }
  }, [location.search]);

  const fetchFilters = () => {
    axios.get("/api/options/categories")
      .then(res => setCategories(res.data.data))
      .catch(() => { window.location.href = "/login"; })

    axios.get("/api/options/orderBy")
      .then(res => setOrderByOptions(res.data.data))
      .catch(() => { window.location.href = "/login"; });
  };

  const fetchProducts = (customFilters = filters, customPage = page) => {
    setLoading(true);

    const searchRequest = {
      input: customFilters.input || null,
      categories: customFilters.selectedCategories.length ? customFilters.selectedCategories : null,
      priceFrom: customFilters.priceFrom ? parseFloat(customFilters.priceFrom) : null,
      priceTo: customFilters.priceTo ? parseFloat(customFilters.priceTo) : null,
      ratingFrom: customFilters.ratingFrom ? parseFloat(customFilters.ratingFrom) : null,
      ratingTo: customFilters.ratingTo ? parseFloat(customFilters.ratingTo) : null,
      orderBy: customFilters.orderBy ? parseInt(customFilters.orderBy, 10) : null,
      page: customPage,
      pageSize: pageSize,
    };

    axios.post("/api/products/search", searchRequest)
      .then(res => {
        const raw = res.data.data;
        setProducts(raw);
        setHasMore(raw.length === pageSize);
      })
      .catch(() => { window.location.href = "/login"; })
      .finally(() => setLoading(false));
  };

  const handleSearch = () => {
    const params = new URLSearchParams();

    if (filters.input) params.set("input", filters.input);
    filters.selectedCategories.forEach(cat => params.append("categories", cat));
    if (filters.priceFrom) params.set("priceFrom", filters.priceFrom);
    if (filters.priceTo) params.set("priceTo", filters.priceTo);
    if (filters.ratingFrom) params.set("ratingFrom", filters.ratingFrom);
    if (filters.ratingTo) params.set("ratingTo", filters.ratingTo);
    if (filters.orderBy) params.set("orderBy", filters.orderBy);
    params.set("page", "1");

    navigate(`/home?${params.toString()}`);
  };

  const handlePageChange = (newPage) => {
    const params = new URLSearchParams(location.search);
    params.set("page", newPage.toString());
    navigate(`/home?${params.toString()}`);
  };

  if (loading) {
    return <div className="home-loading">Loading products...</div>;
  }

  return (
    <div className="home-page">
      <ProductFilters
        filters={filters}
        setFilters={setFilters}
        categories={categories}
        orderByOptions={orderByOptions}
        onSearch={handleSearch}
        isOpen={false}
      />


      {!loading && searched && (
        products.length > 0 ? (
          <div className="home-product-list">
            {products.map(product => (
              <div
                key={product.id}
                onClick={() => navigate(`/product/${product.id}`)}
                className="home-product-card"
              >
                {product.image ? (
                  <div className="home-product-card-image-wrapper">
                    <img
                      src={`data:image/jpeg;base64,${product.image}`}
                      alt={product.name}
                      className="home-product-card-image"
                    />
                  </div>
                ) : (
                  <div className="home-product-card-noimage">No Image</div>
                )}
                <div className="home-product-card-name">{product.name}</div>

                <div className="home-product-card-rating">
                  <div>
                    {Array.from({ length: 5 }, (_, i) => (
                      <span key={i}>
                        {i < Math.round(product.rating || 0) ? '★' : '☆'}
                      </span>
                    ))}
                  </div>
                  <div className="home-product-card-rating-count">
                    ({product.ratingCount || 0})
                  </div>
                </div>

                <div className="home-product-card-price">
                  {product.price}€
                </div>
              </div>
            ))}
          </div>
        ) : (
          <div className="home-no-products">No products found.</div>
        )
      )}

      {!loading && searched && products.length > 0 && (
        <div className="home-pagination-controls">
          <button
            disabled={page === 1}
            onClick={() => handlePageChange(page - 1)}
          >
            Previous
          </button>
          <span>Page {page}</span>
          <button
            disabled={!hasMore}
            onClick={() => handlePageChange(page + 1)}
          >
            Next
          </button>
        </div>
      )}
    </div>
  );
}

export default Home;
