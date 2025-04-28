import { useEffect, useState, useRef } from "react";
import axios from "axios";
import MultiSelectDropdown from "./MultiSelectDropdown";
import "../css/ProductCreateEdit.css";
import { useNotification } from "../contexts/NotificationContext";

function ProductCreateEdit({ product, onClose, onSave }) {
  const { notify } = useNotification();
  const [name, setName] = useState(product.name || "");
  const [description, setDescription] = useState(product.description || "");
  const [price, setPrice] = useState(product.price || "");
  const [categories, setCategories] = useState([]);
  const [selectedCategories, setSelectedCategories] = useState(product.categories ? product.categories.map(c => c.value || c) : []);
  const [image, setImage] = useState(product.image || null);
  const [previewImage, setPreviewImage] = useState(product.image ? `data:image/jpeg;base64,${product.image}` : null);
  const [loadingCategories, setLoadingCategories] = useState(true);
  const [saving, setSaving] = useState(false);

  const fileInputRef = useRef(null);

  useEffect(() => {
    axios.get("/api/options/categories")
      .then(res => {
        setCategories(res.data.data);
      })
      .catch(() => {
        window.location.href = "/login";
        notify("Session expired", "error");
      })
      .finally(() => setLoadingCategories(false));
  }, []);

  const handleImageChange = (e) => {
    const file = e.target.files[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = () => {
        const base64Data = reader.result.split(",")[1];
        setImage(base64Data);
        setPreviewImage(reader.result);
      };
      reader.readAsDataURL(file);
    }
  };

  const handleClearImage = () => {
    setImage(null);
    setPreviewImage(null);
    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!name.trim()) {
      notify("Product name is required.", "error");
      return;
    }
    if (!price || isNaN(price) || parseFloat(price) <= 0) {
      notify("Product price must be a number greater than 0.", "error");
      return;
    }

    setSaving(true);

    const payload = {
      name,
      description,
      price: parseFloat(price),
      categories: selectedCategories,
      image: image,
    };

    try {
      if (product.id) {
        await axios.put(`/api/admin/products/${product.id}`, payload, {
          headers: { 'Content-Type': 'application/json' }
        });
        notify("Product updated successfully!", "success");
      } else {
        await axios.post("/api/admin/products", payload, {
          headers: { 'Content-Type': 'application/json' }
        });
        notify("Product created successfully!", "success");
      }
      onSave();
    } catch (err) {
      window.location.href = "/login";
      notify("Session expired", "error");
      onClose();
    } finally {
      setSaving(false);
    }
  };

  if (loadingCategories) {
    return (
      <div className="product-create-edit">
        <h3>{product.id ? "Edit Product" : "Create New Product"}</h3>
        <div className="prod-loading-content">
          Loading settings...
        </div>
      </div>
    );
  }

  return (
    <div className="product-create-edit">
      <h3>{product.id ? "Edit Product" : "Create New Product"}</h3>
      <form onSubmit={handleSubmit}>
        <div className="product-form-grid">

          <div className="product-form-group">
            <label>Name:</label>
            <input type="text" value={name} onChange={e => setName(e.target.value)} />
          </div>

          <div className="product-form-group">
            <label>Price (€):</label>
            <input
              type="number"
              step="0.01"
              value={price}
              onChange={e => setPrice(e.target.value)}
            />
          </div>

          <div className="product-form-group form-span-2">
            <label>Description:</label>
            <textarea value={description} onChange={e => setDescription(e.target.value)} />
          </div>

          <div className="product-form-group form-span-2">
            <label>Categories:</label>
            <MultiSelectDropdown
              options={categories}
              selectedValues={selectedCategories}
              onChange={setSelectedCategories}
              placeholder="Select categories..."
            />
          </div>

          <div className="product-form-group form-span-2">
            <div className="product-upload-wrapper">
              <div className="product-upload-controls">
                <button
                  type="button"
                  className="product-upload-button"
                  onClick={() => fileInputRef.current.click()}
                >
                  Select Image
                </button>
                <input
                  type="file"
                  accept="image/*"
                  onChange={handleImageChange}
                  ref={fileInputRef}
                  className="product-upload-input"
                />
              </div>

              {previewImage && (
                <div className="product-image-preview">
                  <img src={previewImage} alt="Preview" />
                  <button type="button" onClick={handleClearImage}>Clear Image</button>
                </div>
              )}
            </div>
          </div>

        </div>

        <div className="product-form-actions">
          <button type="button" onClick={onClose} className="product-cancel-button">Cancel</button>
          <button type="submit" disabled={saving} className="product-save-button">
            {saving ? "Saving..." : "Save"}
          </button>
        </div>
      </form>
    </div>
  );
}

export default ProductCreateEdit;
