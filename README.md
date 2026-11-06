
# DiNet.HashSimilarityTK

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![NuGet Version](https://img.shields.io/nuget/v/DiNet.HashSimilarityTK.Cli.svg)](https://www.nuget.org/packages/DiNet.HashSimilarityTK.Cli/)

> A lightweight CLI toolkit for discovering near-duplicate code and structural similarities across large codebases.

---

## 📌 Overview

**DiNet.HashSimilarityTK** helps developers detect structural redundancies, scattered domain models, and duplicate logic across repositories—even when class names or formatting differ. It enables engineering teams to enforce the [DRY (Don't Repeat Yourself)](https://en.wikipedia.org/wiki/Don%27t_repeat_yourself) principle and streamline refactoring efforts.

---


## 🚀 Available CLI Commands

This toolkit exposes two primary CLI tools via route aliases. Options can be passed using kebab-case flags (e.g., `--shingle-size 4`), and unflagged positional arguments represent the target path.

### ⚙️ Common Parameters

These parameters are shared across both commands to configure the core analysis engine:

* **Positional Root Path** (`RootPath`, `string?`, Default: `null`): Target directory path to analyze (passed without a flag prefix, e.g., `./src`).
* **`--shingle-size`** (`int`, Default: `4`): Number of byte tokens grouped into a single shingle for tokenization.
* **`--num-hashes`** (`int`, Default: `128`): Total number of MinHash functions generated for signature construction.
* **`--chunk-step`** (`int`, Default: `16`): Signature split size defining how many shingle hashes are combined into a single key to determine its LSH bucket.
* **`--seed`** (`uint`, Default: `123456789`): Seed value for random hash functions to ensure deterministic execution runs.
* **`--ignore`** (`string?`, Default: `null`): Path to a `.gitignore`-style file (e.g., `.hsimignore`) to filter out build artifacts and unwanted binaries (`bin/`, `obj/`, etc.).

---

### 0. Help & Command Discovery (`help`)

Displays the built-in help guide listing all available routes, commands, and parameter flags.

```bash
hsim help
```

### 1. Find Top N File Similarities (`top`)

Finds and ranks the top similar file pairs across the target directory using MinHash/LSH signatures and Jaccard similarity.

```bash
# Basic usage
hsim top ./src --top 10

# Advanced usage
hsim top ./src --top 5 --shingle-size 4 --num-hashes 128 --chunk-step 16 --seed 123456789 --ignore .hsimignore

```

**Unique Parameters:**

* **`--top`** (`int`, Default: `5`): Core query option specifying the number of top similar file pairs to return in the output.

---

### 2. Longest Near-Duplicate Sequence Detector (`seq`)

Identifies the longest matching continuous line sequences across different files or within a single file.

```bash
# Basic usage
hsim seq ./src

# Advanced usage
hsim seq ./src --compare-different-documents --max-bucket-size-limit 50

```

**Unique Parameters:**

* **`--max-bucket-size-limit`** (`uint`, Default: `100`): Empirical threshold to skip oversized LSH buckets containing repetitive noise, preventing $O(N^2)$ execution slowdowns.
* **`--compare-different-documents`** (`bool`, Default: `false`): Flag switch. When set to `true`, limits matching exclusively between distinct files. When `false`, enables finding identical repeated sequences inside the same file at different line offsets.

---


## 🏗️ Architecture & Solution Hierarchy

The repository is structured into modular engines separating application logic, core processing, and infrastructure:

* **Apps**: Entry point implementations.
  * `DiNet.HashSimilarityTK.Cli`
* **FileProcessingEngine**: File streaming, tokenization, and code slicing.
  * `DiNet.HashSimilarityTK.FileProcessing.Core`
  * `DiNet.HashSimilarityTK.FileProcessing.Infrastructure`
* **HashSimilarityEngine**: Similarity calculation core algorithms.
  * `DiNet.HashSimilarityTK.Core`
  * `DiNet.HashSimilarityTK.Infrastructure`
* **MetricsEngine**: Quantitative analysis and distance scoring.
  * `DiNet.HashSimilarityTK.MetricsEngine.App`
  * `DiNet.HashSimilarityTK.MetricsEngine.Core`
  * `DiNet.HashSimilarityTK.MetricsEngine.Infrastructure`

---

## ⚙️ How It Works (The Engine)

At its core, the similarity calculation engine leverages **[MinHash](https://en.wikipedia.org/wiki/MinHash)** and **[Locality-Sensitive Hashing (LSH)](https://en.wikipedia.org/wiki/Locality-sensitive_hashing)** to efficiently uncover close duplicates at scale.

### Algorithm Highlights
* **MinHash**: Estimates the Jaccard similarity index between code tokens without requiring exhaustive pair-wise comparisons.
* **LSH**: Hashes high-dimensional code representations into buckets, drastically reducing lookup time to linear or sub-quadratic complexity.

---

## 🖥️ CLI Routing & Command Architecture

The toolkit uses a custom reflection-driven routing engine built specifically for single-execution CLI applications. Handlers are loosely coupled and bound via route words.

Check out the [DiNet.CliToolkit Repository](https://github.com/DifolderyXXL/DiNet.CliToolkit)

---

## 🎯 Use Cases

* **Consolidating Scattered Domain Models**: Identify class definitions that perform identical operations despite having different names or namespaces.
* **Refactoring & Technical Debt Prevention**: Pinpoint exact locations where helper methods or shared libraries should be introduced to eliminate duplication before it spreads.
* **Cross-Team Alignment**: Uncover feature overlaps introduced independently by different contributors working on a shared codebase.

---

## 📚 References & Citations

1. **MinHash Algorithm**. [Wikipedia Overview](https://en.wikipedia.org/wiki/MinHash)
2. **Locality-Sensitive Hashing**. [Medium Article](https://medium.com/@jonathankoren/near-duplicate-detection-b6694e807f7a)
3. **Jaccard Similarity Coefficient**. [Wikipedia Overview](https://en.wikipedia.org/wiki/Jaccard_index)

