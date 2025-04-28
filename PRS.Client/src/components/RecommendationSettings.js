import { useState, useEffect } from "react";
import axios from "axios";
import "../css/RecommendationSettings.css";
import { useNotification } from "../contexts/NotificationContext";

function RecommendationSettings({ onClose }) {
  const { notify } = useNotification();
  const [localSettings, setLocalSettings] = useState(null);
  const [saving, setSaving] = useState(false);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchSettings();
  }, []);

  const fetchSettings = async () => {
    try {
      const response = await axios.get("/api/settings");
      if (response.data.success) {
        setLocalSettings(response.data.data);
      }
    } catch (err) {
      onClose();
      window.location.href = "/login";
      notify("Session expired", "error");
    } finally {
      setLoading(false);
    }
  };

  const saveSettings = async () => {
    try {
      setSaving(true);
      await axios.post("/api/settings", localSettings);
      notify("Settings created successfully!", "success");
    } catch (err) {
      window.location.href = "/login";
      notify("Session expired", "error");
    } finally {
      setSaving(false);
      onClose();
    }
  };

  const handleChange = (field, value) => {
    setLocalSettings(prev => ({ ...prev, [field]: value }));
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    saveSettings();
  };

  if (loading) {
    return (
      <div className="rec-edit">
        <h3>Recommendation Settings</h3>
        <div className="rec-loading-content">
          Loading settings...
        </div>
      </div>
    );
  }

  return (
    <div className="rec-edit">
      <h3>Recommendation Settings</h3>
      <form onSubmit={handleSubmit}>
        <div className="rec-form-grid">

          <div className="rec-form-group rec-form-span-2">
            <label>
              <input
                type="checkbox"
                checked={localSettings.useContent}
                onChange={e => handleChange("useContent", e.target.checked)}
              /> Use Content-based Filtering
            </label>
          </div>

          <div className="rec-form-group rec-form-span-2">
            <label>
              <input
                type="checkbox"
                checked={localSettings.useCollaborative}
                onChange={e => handleChange("useCollaborative", e.target.checked)}
              /> Use Collaborative Filtering
            </label>
          </div>

          {localSettings.useContent && (
            <>
              <div className="rec-form-group">
                <label>Category Weight:</label>
                <input
                  type="number"
                  step="0.01"
                  value={localSettings.categoryWeight}
                  onChange={e => handleChange("categoryWeight", parseFloat(e.target.value))}
                />
              </div>

              <div className="rec-form-group">
                <label>Price Weight:</label>
                <input
                  type="number"
                  step="0.01"
                  value={localSettings.priceWeight}
                  onChange={e => handleChange("priceWeight", parseFloat(e.target.value))}
                />
              </div>
            </>
          )}

          {localSettings.useCollaborative && (
            <>
              <div className="rec-form-group">
                <label>Browse Weight:</label>
                <input
                  type="number"
                  step="0.01"
                  value={localSettings.browseWeight}
                  onChange={e => handleChange("browseWeight", parseFloat(e.target.value))}
                />
              </div>

              <div className="rec-form-group">
                <label>View Detail Weight:</label>
                <input
                  type="number"
                  step="0.01"
                  value={localSettings.viewWeight}
                  onChange={e => handleChange("viewWeight", parseFloat(e.target.value))}
                />
              </div>

              <div className="rec-form-group">
                <label>Cart Weight:</label>
                <input
                  type="number"
                  step="0.01"
                  value={localSettings.cartWeight}
                  onChange={e => handleChange("cartWeight", parseFloat(e.target.value))}
                />
              </div>

              <div className="rec-form-group">
                <label>Purchase Weight:</label>
                <input
                  type="number"
                  step="0.01"
                  value={localSettings.purchaseWeight}
                  onChange={e => handleChange("purchaseWeight", parseFloat(e.target.value))}
                />
              </div>

              <div className="rec-form-group">
                <label>Rating Weight:</label>
                <input
                  type="number"
                  step="0.01"
                  value={localSettings.ratingWeight}
                  onChange={e => handleChange("ratingWeight", parseFloat(e.target.value))}
                />
              </div>
            </>
          )}

        </div>

        <div className="rec-form-actions">
          <button type="button" onClick={onClose} className="rec-cancel-button">Cancel</button>
          <button type="submit" disabled={saving} className="rec-save-button">
            {saving ? "Saving..." : "Save"}
          </button>
        </div>
      </form>
    </div>
  );
}

export default RecommendationSettings;
