document.addEventListener('DOMContentLoaded', function() {
    // Elements
    const toolButtons = document.querySelectorAll('.tool-btn');
    const payloadForm = document.getElementById('payload-form');
    const payloadInput = document.getElementById('payload-input');
    const samplesOptions = document.getElementById('samples-options');
    const convertOptions = document.getElementById('convert-options');
    const resultSection = document.getElementById('result-section');
    const resultOutput = document.getElementById('result-output');
    const copyBtn = document.getElementById('copy-btn');
    const downloadBtn = document.getElementById('download-btn');
    const loadingIndicator = document.getElementById('loading');
    const exampleButtons = document.querySelectorAll('.example-btn');
    
    // API endpoint base URL
    const apiBaseUrl = '/api/v1/data';
    
    // Example payloads
    const examples = {
        json: `{
  "users": [
    {
      "id": 1,
      "name": "John Smith",
      "email": "john.smith@example.com",
      "age": 32,
      "address": {
        "street": "123 Main St",
        "city": "New York",
        "zipCode": "10001",
        "country": "USA"
      },
      "phoneNumber": "+1-555-123-4567",
      "roles": ["admin", "user"],
      "isActive": true,
      "registeredAt": "2023-01-15T08:30:00Z"
    },
    {
      "id": 2,
      "name": "Jane Doe",
      "email": "jane.doe@example.com",
      "age": 28,
      "address": {
        "street": "456 Park Ave",
        "city": "Boston",
        "zipCode": "02108",
        "country": "USA"
      },
      "phoneNumber": "+1-555-987-6543",
      "roles": ["user"],
      "isActive": true,
      "registeredAt": "2023-03-20T14:45:00Z"
    }
  ]
}`,
        xml: `<?xml version="1.0" encoding="UTF-8"?>
<users>
  <user id="1">
    <name>John Smith</name>
    <email>john.smith@example.com</email>
    <age>32</age>
    <address>
      <street>123 Main St</street>
      <city>New York</city>
      <zipCode>10001</zipCode>
      <country>USA</country>
    </address>
    <phoneNumber>+1-555-123-4567</phoneNumber>
    <roles>
      <role>admin</role>
      <role>user</role>
    </roles>
    <isActive>true</isActive>
    <registeredAt>2023-01-15T08:30:00Z</registeredAt>
  </user>
  <user id="2">
    <name>Jane Doe</name>
    <email>jane.doe@example.com</email>
    <age>28</age>
    <address>
      <street>456 Park Ave</street>
      <city>Boston</city>
      <zipCode>02108</zipCode>
      <country>USA</country>
    </address>
    <phoneNumber>+1-555-987-6543</phoneNumber>
    <roles>
      <role>user</role>
    </roles>
    <isActive>true</isActive>
    <registeredAt>2023-03-20T14:45:00Z</registeredAt>
  </user>
</users>`,
        yaml: `users:
  - id: 1
    name: John Smith
    email: john.smith@example.com
    age: 32
    address:
      street: 123 Main St
      city: New York
      zipCode: '10001'
      country: USA
    phoneNumber: +1-555-123-4567
    roles:
      - admin
      - user
    isActive: true
    registeredAt: '2023-01-15T08:30:00Z'
  - id: 2
    name: Jane Doe
    email: jane.doe@example.com
    age: 28
    address:
      street: 456 Park Ave
      city: Boston
      zipCode: '02108'
      country: USA
    phoneNumber: +1-555-987-6543
    roles:
      - user
    isActive: true
    registeredAt: '2023-03-20T14:45:00Z'`
    };
    
    // Tool selection
    toolButtons.forEach(button => {
        button.addEventListener('click', function() {
            // Update active button
            toolButtons.forEach(btn => btn.classList.remove('active'));
            this.classList.add('active');
            
            // Update form action
            const tool = this.dataset.tool;
            payloadForm.action = `${apiBaseUrl}/${tool}`;
            
            // Show/hide tool-specific options
            samplesOptions.style.display = tool === 'samples' ? 'block' : 'none';
            convertOptions.style.display = tool === 'convert' ? 'block' : 'none';
        });
    });
    
    // Load example payloads
    exampleButtons.forEach(button => {
        button.addEventListener('click', function() {
            const format = this.dataset.format;
            payloadInput.value = examples[format] || '';
            
            // Select the corresponding format radio button
            document.querySelector(`input[name="format"][value="${format}"]`).checked = true;
        });
    });
    
    // Form submission
    payloadForm.addEventListener('submit', async function(e) {
        e.preventDefault();
        
        // Get active tool
        const activeTool = document.querySelector('.tool-btn.active').dataset.tool;
        
        // Prepare request data
        let requestData = {
            payload: payloadInput.value
        };
        
        // Add tool-specific data
        if (activeTool === 'samples') {
            requestData.count = document.getElementById('samples-count').value;
            requestData.criteria = document.getElementById('samples-criteria').value;
        } else if (activeTool === 'convert') {
            requestData.targetFormat = document.getElementById('target-format').value;
        }
        
        // Show loading indicator
        loadingIndicator.style.display = 'flex';
        
        try {
            console.log(`Sending request to ${apiBaseUrl}/${activeTool} with data:`, requestData);
            // Send request
            const response = await fetch(`${apiBaseUrl}/${activeTool}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(requestData)
            });
            
            if (!response.ok) {
                console.error(`Request failed with status: ${response.status} ${response.statusText}`);
                throw new Error(`Error: ${response.status} ${response.statusText}`);
            }
            
            const data = await response.json();
            
            // Display result
            resultOutput.textContent = data.result;
            resultSection.style.display = 'block';
            
            // Scroll to result
            resultSection.scrollIntoView({ behavior: 'smooth' });
        } catch (error) {
            alert(`An error occurred: ${error.message}`);
            console.error('Error:', error);
        } finally {
            // Hide loading indicator
            loadingIndicator.style.display = 'none';
        }
    });
    
    // Copy result to clipboard
    copyBtn.addEventListener('click', function() {
        navigator.clipboard.writeText(resultOutput.textContent)
            .then(() => {
                const originalText = this.textContent;
                this.textContent = 'Copied!';
                setTimeout(() => {
                    this.textContent = originalText;
                }, 2000);
            })
            .catch(err => {
                console.error('Failed to copy text: ', err);
                alert('Failed to copy text to clipboard');
            });
    });
    
    // Download result
    downloadBtn.addEventListener('click', function() {
        const activeTool = document.querySelector('.tool-btn.active').dataset.tool;
        const blob = new Blob([resultOutput.textContent], { type: 'text/plain' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `dataforge_${activeTool}_result.txt`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    });
});
