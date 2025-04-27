import { createContext, useState, useContext } from "react";
import axios from "../api/axios";

const CartContext = createContext();

export function CartProvider({ children }) {
  const [cartCount, setCartCount] = useState(0);

  const fetchCartCount = async () => {
    try {
      const res = await axios.get("/api/cart");
      const items = res.data.data || [];
      const totalCount = items.reduce((sum, item) => sum + item.count, 0);
      setCartCount(totalCount);
    } catch {
      setCartCount(0);
    }
  };

  const clearCartCount = () => {
    setCartCount(0);
  };

  return (
    <CartContext.Provider value={{ cartCount, fetchCartCount, clearCartCount }}>
      {children}
    </CartContext.Provider>
  );
}

export function useCart() {
  return useContext(CartContext);
}

export default CartContext;
