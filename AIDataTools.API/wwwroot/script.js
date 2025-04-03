angular.module('aidataToolsApp', [])
.controller('DataController', ['$scope', '$http', function($scope, $http) {
    $scope.selectedTool = 'anonymize';
    $scope.selectedFormat = 'json';
    $scope.payload = '';
    $scope.samplesCount = 3;
    $scope.samplesCriteria = '';
    $scope.targetFormat = 'json';
    $scope.result = '';
    $scope.loading = false;
    $scope.freeformText = '';

    const apiBaseUrl = '/api/v1/data';

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

    $scope.selectTool = function(tool) {
        $scope.selectedTool = tool;
    };

    $scope.loadExample = function(format) {
        $scope.payload = examples[format] || '';
        $scope.selectedFormat = format;
    };

    $scope.processPayload = function() {
        $scope.loading = true;
        $scope.result = '';

        let requestData = {
            payload: $scope.payload
        };

        if ($scope.selectedTool === 'samples') {
            requestData.count = $scope.samplesCount;
            requestData.criteria = $scope.samplesCriteria;
        } else if ($scope.selectedTool === 'convert') {
            requestData.targetFormat = $scope.targetFormat;
        } else if ($scope.selectedTool === 'freeform') {
            requestData.text = $scope.freeformText;
        }

        let endpoint = `${apiBaseUrl}/${$scope.selectedTool}`;

        $http({
            method: 'POST',
            url: endpoint,
            headers: {
                'Content-Type': 'application/json'
            },
            data: requestData
        }).then(function(response) {
            $scope.result = response.data.result;
        }).catch(function(error) {
            console.error('Error:', error);
            $scope.result = 'Error processing request.';
        }).finally(function() {
            $scope.loading = false;
        });
    };

    $scope.copyResult = function() {
        navigator.clipboard.writeText($scope.result)
            .then(function() {
                alert('Result copied to clipboard!');
            })
            .catch(function(err) {
                console.error('Could not copy text: ', err);
                alert('Could not copy text to clipboard');
            });
    };

    $scope.downloadResult = function() {
        const blob = new Blob([$scope.result], { type: 'text/plain' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `dataforge_${$scope.selectedTool}_result.txt`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    };

    $scope.isModelAvailable = false; // Initially assume the model is available

    $scope.downloadModel = function() {
        $scope.loading = true;
        $http({
            method: 'GET',
            url: `${apiBaseUrl}/downloadModel`
        }).then(function(response) {
            $scope.isModelAvailable = true;
            alert('Model downloaded successfully!');
        }).catch(function(error) {
            console.error('Error:', error);
            alert('Error downloading model.');
        }).finally(function() {
            $scope.loading = false;
        });
    };
}]);
