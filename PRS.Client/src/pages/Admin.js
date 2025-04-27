import { useEffect, useState } from "react";
import axios from "axios";
import ProductCreateEdit from "../components/ProductCreateEdit";
import ProductFilters from "../components/ProductFilters";
import { Pencil, Trash2 } from "lucide-react";
import "../css/Admin.css";

function Admin() {
    const [products, setProducts] = useState([]);
    const [loading, setLoading] = useState(true);
    const [creating, setCreating] = useState(false);
    const [editingProduct, setEditingProduct] = useState(null);

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

    useEffect(() => {
        fetchProducts();
        axios.get("/api/options/categories")
            .then(res => setCategories(res.data.data))
            .catch(() => alert("Failed to load categories"));

        axios.get("/api/options/orderBy")
            .then(res => setOrderByOptions(res.data.data))
            .catch(() => alert("Failed to load order by options"));
    }, []);

    const fetchProducts = () => {
        setLoading(true);
        const searchRequest = {
            input: filters.input || null,
            categories: filters.selectedCategories.length ? filters.selectedCategories : null,
            priceFrom: filters.priceFrom ? parseFloat(filters.priceFrom) : null,
            priceTo: filters.priceTo ? parseFloat(filters.priceTo) : null,
            ratingFrom: filters.ratingFrom ? parseFloat(filters.ratingFrom) : null,
            ratingTo: filters.ratingTo ? parseFloat(filters.ratingTo) : null,
            orderBy: filters.orderBy ? parseInt(filters.orderBy, 10) : null,
        };

        axios.post("/api/admin/products/search", searchRequest)
            .then(res => setProducts(res.data.data))
            .catch(() => alert("Failed to load products"))
            .finally(() => setLoading(false));
    };

    const handleEdit = (product) => {
        setEditingProduct(product);
        setCreating(true);
    };

    const handleDelete = (id) => {
        if (!window.confirm("Are you sure you want to delete this product?")) return;

        axios.delete(`/api/admin/products/${id}`)
            .then(() => fetchProducts())
            .catch(() => alert("Failed to delete product"));
    };

    if (loading) {
        return <div className="admin-loading">Loading products...</div>;
    }

    return (
        <div className="admin-page">
            <ProductFilters
                filters={filters}
                setFilters={setFilters}
                categories={categories}
                orderByOptions={orderByOptions}
                onSearch={fetchProducts}
                isOpen={false}
            />

            <div className="admin-create-wrapper">
                <button
                    className="admin-create-button"
                    onClick={() => {
                        setEditingProduct(null);
                        setCreating(true);
                    }}
                >
                    Create New Product
                </button>
            </div>

            <div className="admin-product-list">
                {products.length === 0 ? (
                    <p className="admin-no-products">No products found.</p>
                ) : (
                    products.map(product => (
                        <div key={product.id} className="admin-product-item">
                            <div className="admin-product-left">
                                {product.image ? (
                                    <img
                                        src={`data:image/jpeg;base64,${product.image}`}
                                        alt={product.name}
                                        className="admin-product-thumb"
                                    />
                                ) : (
                                    <div className="admin-product-placeholder" />
                                )}
                                <div className="admin-product-info">
                                    <div className="admin-product-name">{product.name}</div>
                                    <div className="admin-product-price">{product.price}€</div>
                                </div>
                            </div>

                            <div className="admin-product-actions">
                                <button
                                    className="admin-icon-button admin-edit-button"
                                    onClick={() => handleEdit(product)}
                                    title="Edit"
                                >
                                    <Pencil size={24} />
                                </button>
                                <button
                                    className="admin-icon-button admin-delete-button"
                                    onClick={() => handleDelete(product.id)}
                                    title="Delete"
                                >
                                    <Trash2 size={24} />
                                </button>
                            </div>
                        </div>
                    ))
                )}
            </div>

            {creating && (
                <div className="admin-modal-overlay">
                    <div className="admin-modal-content">
                        <ProductCreateEdit
                            product={editingProduct || {}}
                            onClose={() => {
                                setCreating(false);
                                setEditingProduct(null);
                            }}
                            onSave={() => {
                                fetchProducts();
                                setCreating(false);
                                setEditingProduct(null);
                            }}
                        />
                    </div>
                </div>
            )}
        </div>
    );
}

export default Admin;
