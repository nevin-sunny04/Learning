import pandas as pd

# Read the CSV file into a DataFrame
df = pd.read_csv(r'C:\Users\nsunny\Downloads\msiList\nhs-20240605-130951.csv')

# Add a new column to concatenate product and device names
df['Concatenated'] = df['ProductName'] + df['MachineName']

# Find the latest version for each product-device combination
latest_versions = df.groupby('Concatenated')['ProductVersion'].max().reset_index()

# Merge the latest versions back to the original DataFrame
df = pd.merge(df, latest_versions, on=['Concatenated', 'ProductVersion'], how='inner')

# Remove the 'Concatenated' column
df = df.drop(columns=['Concatenated'])

# Write the updated DataFrame back to a CSV file
df.to_csv(r'C:\Users\nsunny\Downloads\msiList\updated_file.csv', index=False)
