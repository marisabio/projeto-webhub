using HtmlAgilityPack;
using Oracle.ManagedDataAccess.Client;

namespace WebHub;

class Program
{
    static async Task Main(string[] args)
    {
        var urls = new List<string>
        {
            "https://www.mercadolivre.com.br/impressora-fotografica-ecotank-l8050-wi-fi-bivolt-epson-cor-preto-110v220v/p/MLB21696226?pdp_filters=item_id:MLB3210495840#is_advertising=true&searchVariation=MLB21696226&position=2&search_layout=grid&type=pad&tracking_id=7dcb62cb-3f0e-45f3-82dc-e5e99c67217c&is_advertising=true&ad_domain=VQCATCORE_LST&ad_position=2&ad_click_id=ZTIzM2FmNjYtMzk0Yi00MzRjLTg0MmEtMjQ2YzEwMTJiYmJm",
            "https://www.mercadolivre.com.br/impressora-epson-de-fotos-ecotank-l8180-wi-fi-eps01-cor-preto-100v/p/MLB18462374#reco_item_pos=2&reco_backend=univb-pdp-buybox&reco_backend_type=low_level&reco_client=pdp-v2p&reco_id=d842343e-faf9-4e2a-8d00-e2f463a0b384&reco_backend_model=univb",
            "https://www.mercadolivre.com.br/impressora-a-cor-multifuncional-epson-ecotank-l3250-com-wifi-preta-110v/p/MLB18446928#reco_item_pos=0&reco_backend=univb-pdp-buybox&reco_backend_type=low_level&reco_client=pdp-v2p&reco_id=d842343e-faf9-4e2a-8d00-e2f463a0b384&reco_backend_model=univb"
        };

        var client = new HttpClient();

        foreach (var url in urls)
        {
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            
            var doc = new HtmlDocument();
            doc.LoadHtml(content);
            
            var titulo = doc.DocumentNode.SelectSingleNode("/html/body/main/div[2]/div[3]/div[2]/div[1]/div[1]/div/div[1]/div[2]/div[1]/div/div[2]/h1").InnerText?.Trim();
            var preco = doc.DocumentNode.SelectSingleNode("/html/body/main/div[2]/div[3]/div[2]/div[1]/div[1]/div/div[1]/div[2]/div[3]/div[1]/div[1]/span/span/span[2]").InnerText?.Trim();
            
            await InserirNoBancoDeDados(url, titulo, preco);
        }
    }

    static async Task InserirNoBancoDeDados(string url, string titulo, string preco)
    {
        var connectionString = "User Id=RM96906;Password=160898;Data Source=oracle.fiap.com.br:1521/ORCL;";

        using (var connection = new OracleConnection(connectionString))
        {
            await connection.OpenAsync();
            using (var command = new OracleCommand("INSERT INTO t_produto_web (titulo, preco, url) VALUES (:titulo, :preco, :url)", connection))
            {
                command.Parameters.Add("titulo", OracleDbType.NVarchar2).Value = titulo;
                command.Parameters.Add("preco", OracleDbType.NVarchar2).Value = preco;
                command.Parameters.Add("url", OracleDbType.Varchar2).Value = url; 

                await command.ExecuteNonQueryAsync();
            }
        }
    }
}