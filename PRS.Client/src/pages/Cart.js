import { useEffect, useState } from "react";
import axios from "axios";
import { useNavigate } from "react-router-dom";
import { useCart } from "../contexts/CartContext";
import "../css/Cart.css";

function Cart() {
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
            .then(res => {
                setCartItems(res.data.data);
            })
            .catch(() => {
                alert("Failed to load cart");
                navigate("/home");
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
                alert("Failed to update cart");
            });
    };

    const removeItem = (productId) => {
        axios.post("/api/cart", { ProductId: productId, Count: 0 })
            .then(() => {
                setCartItems(prev => prev.filter(item => item.productId !== productId));
                fetchCartCount();
            })
            .catch(() => {
                alert("Failed to remove item");
            });
    };

    const handleBuy = () => {
        setBuying(true);
        axios.post("/api/cart/buy")
            .then(() => {
                alert("Purchase successful!");
                fetchCart();
                fetchCartCount();
            })
            .catch(() => {
                alert("Failed to complete purchase");
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
            <h2>Your Cart</h2>

            {cartItems.length === 0 ? (
                <p>Your cart is empty.</p>
            ) : (
                <>
                    <div className="cart-items">
                        {cartItems.map(item => (
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
                                    <div className="cart-item-price">{item.price}€</div>
                                </div>

                                <div className="cart-item-quantity" onClick={e => e.stopPropagation()}>
                                    <button onClick={() => updateItemCount(item.productId, item.count - 1)}>-</button>
                                    <span>{item.count}</span>
                                    <button onClick={() => updateItemCount(item.productId, item.count + 1)}>+</button>
                                </div>

                                <button
                                    className="cart-item-remove"
                                    onClick={(e) => {
                                        e.stopPropagation();
                                        removeItem(item.productId);
                                    }}
                                >
                                    Remove
                                </button>
                            </div>
                        ))}
                    </div>

                    <div className="cart-total">
                        Total: {totalPrice.toFixed(2)}€
                    </div>

                    <div className="cart-buy">
                        <button
                            onClick={handleBuy}
                            disabled={buying}
                            className="cart-buy-button"
                        >
                            {buying ? "Processing..." : "Buy Now"}
                        </button>
                    </div>
                </>
            )}
        </div>
    );
}

export default Cart;
