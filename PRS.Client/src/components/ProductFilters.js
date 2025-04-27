import { useState } from "react";
import MultiSelectDropdown from "./MultiSelectDropdown";
import { Search, ChevronUp, ChevronDown } from "lucide-react";
import "../css/ProductFilters.css";

function ProductFilters({ filters, setFilters, categories, orderByOptions, onSearch, isOpen }) {
  const [open, setOpen] = useState(isOpen);

  const handleClear = () => {
    setFilters({
      input: "",
      selectedCategories: [],
      priceFrom: "",
      priceTo: "",
      ratingFrom: "",
      ratingTo: "",
      orderBy: ""
    });
    onSearch();
  };

  return (
    <div className="product-filters">
      <div className="filter-toolbar">
        <input
          className="search-input"
          type="text"
          placeholder="Search by name or description..."
          value={filters.input}
          onChange={e => setFilters(prev => ({ ...prev, input: e.target.value }))}
        />
        <button className="search-button" onClick={onSearch}>
          <Search size={18} />
        </button>
        <button className="toggle-filters" onClick={() => setOpen(!open)}>
          {open ? <ChevronUp size={18} /> : <ChevronDown size={18} />}
        </button>
      </div>

      {open && (
        <div className="filter-body">
          <div className="product-filters-categories">
            <label>Categories:</label>
            <MultiSelectDropdown
              options={categories}
              selectedValues={filters.selectedCategories}
              onChange={values => setFilters(prev => ({ ...prev, selectedCategories: values }))}
              placeholder="Select categories..."
            />
          </div>

          <div className="filter-row">
            <div className="filter-column">
              <label>Price From:</label>
              <input
                type="number"
                step="0.01"
                min="0.01"
                value={filters.priceFrom}
                onChange={e => setFilters(prev => ({ ...prev, priceFrom: e.target.value }))}
              />
            </div>
            <div className="filter-column">
              <label>Price To:</label>
              <input
                type="number"
                step="0.01"
                min="0.01"
                value={filters.priceTo}
                onChange={e => setFilters(prev => ({ ...prev, priceTo: e.target.value }))}
              />
            </div>
          </div>

          <div className="filter-row">
            <div className="filter-column">
              <label>Rating From:</label>
              <input
                type="number"
                step="1"
                value={filters.ratingFrom}
                onChange={e => setFilters(prev => ({ ...prev, ratingFrom: e.target.value }))}
              />
            </div>
            <div className="filter-column">
              <label>Rating To:</label>
              <input
                type="number"
                step="1"
                value={filters.ratingTo}
                onChange={e => setFilters(prev => ({ ...prev, ratingTo: e.target.value }))}
              />
            </div>
          </div>

          <div className="filter-group">
            <label>Order By:</label>
            <select
              value={filters.orderBy}
              onChange={e => setFilters(prev => ({ ...prev, orderBy: e.target.value }))}
            >
              <option value="">-- Select --</option>
              {orderByOptions.map(opt => (
                <option key={opt.value} value={opt.value}>
                  {opt.label.replace(/([a-z])([A-Z])/g, "$1 $2")}
                </option>
              ))}
            </select>
          </div>

          <div className="filter-actions">
            <button onClick={handleClear}>
              Clear Filters
            </button>
          </div>
        </div>
      )}
    </div>
  );
}

export default ProductFilters;
