import { useEffect, useState } from "react";
import axios from "axios";
import { useNavigate } from "react-router-dom";
import { useCart } from "../contexts/CartContext";
import { Trash2 } from "lucide-react";
import "../css/Cart.css";
import { useNotification } from "../contexts/NotificationContext";

function Cart() {
    const { notify } = useNotification();
    const [cartItems, setCartItems] = useState([]);
    const [loading, setLoading] = useState(true);
    const [buying, setBuying] = useState(false);
    const navigate = useNavigate();
    const { fetchCartCount } = useCart();

    useEffect(() => {
        fetchCart();
    }, []);

    const fetchCart = () => {
        setLoading(true);
        axios.get("/api/cart")
            .then(res => setCartItems(res.data.data))
            .catch(() => {
                window.location.href = "/login";
                notify("Session expired", "error");
            })
            .finally(() => setLoading(false));
    };

    const updateItemCount = (productId, count) => {
        if (count < 1) return;
        axios.post("/api/cart", { ProductId: productId, Count: count })
            .then(() => {
                setCartItems(prev =>
                    prev.map(item =>
                        item.productId === productId ? { ...item, count } : item
                    )
                );
                fetchCartCount();
            })
            .catch(() => {
                window.location.href = "/login";
                notify("Session expired", "error");
            });
    };

    const removeItem = (productId) => {
        axios.post("/api/cart", { ProductId: productId, Count: 0 })
            .then(() => {
                setCartItems(prev => prev.filter(item => item.productId !== productId));
                fetchCartCount();
            })
            .catch(() => {
                window.location.href = "/login";
                notify("Session expired", "error");
            });
    };

    const handleBuy = () => {
        setBuying(true);
        axios.post("/api/cart/buy")
            .then(() => {
                notify("Purchase successful!", "success");
                fetchCart();
                fetchCartCount();
            })
            .catch(() => {
                window.location.href = "/login";
                notify("Session expired", "error");
            })
            .finally(() => {
                setBuying(false);
            });
    };

    if (loading) {
        return <div className="cart-loading">Loading cart...</div>;
    }

    const totalPrice = cartItems.reduce((sum, item) => sum + (item.price * item.count), 0);

    return (
        <div className="cart-page">
            <div className="cart-content">
                <div className="cart-items">
                    {cartItems.length === 0 ? (
                        <div className="cart-empty-message">Your cart is empty.</div>
                    ) : (
                        cartItems.map(item => (
                            <div key={item.productId} className="cart-item" onClick={() => navigate(`/product/${item.productId}`)}>
                                {item.image ? (
                                    <img
                                        src={`data:image/jpeg;base64,${item.image}`}
                                        alt={item.name}
                                        className="cart-item-image"
                                    />
                                ) : (
                                    <div className="cart-item-placeholder">No Image</div>
                                )}

                                <div className="cart-item-info">
                                    <div className="cart-item-name">{item.name}</div>
                                    <div className="cart-item-details">
                                        {item.count} × {item.price.toFixed(2)}€ = {(item.price * item.count).toFixed(2)}€
                                    </div>
                                </div>

                                <div className="cart-item-quantity" onClick={e => e.stopPropagation()}>
                                    <button onClick={() => updateItemCount(item.productId, item.count - 1)}>-</button>
                                    <span>{item.count}</span>
                                    <button onClick={() => updateItemCount(item.productId, item.count + 1)}>+</button>
                                </div>

                                <button
                                    className="cart-icon-button cart-delete-button"
                                    onClick={(e) => {
                                        e.stopPropagation();
                                        removeItem(item.productId);
                                    }}
                                    title="Remove"
                                >
                                    <Trash2 size={20} />
                                </button>
                            </div>
                        ))
                    )}
                </div>

                <div className="cart-summary">
                    <div className="cart-summary-box">
                        <div className="cart-total">Total: {totalPrice.toFixed(2)}€</div>
                        <div className="cart-buy">
                            <button onClick={handleBuy} disabled={buying || cartItems.length === 0} className="cart-buy-button">
                                {buying ? "Processing..." : "Buy Now"}
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}

export default Cart;
