using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace StokTakipOtomasyonu
{
    public partial class frmKategori : Form
    {
        public frmKategori()
        {
            InitializeComponent();
        }

        MySqlConnection baglan = new MySqlConnection("Server=localhost;Database=stok_takip;Uid=root;Pwd=root;");
        bool durum;

        private void kategoriengelle()
        {
            durum = true;

            try
            {
                baglan.Open();
                MySqlCommand komut = new MySqlCommand("SELECT * FROM kategoriler", baglan);
                MySqlDataReader oku = komut.ExecuteReader();
                while (oku.Read())
                {
                    if (txtkategori.Text.Trim().Equals(oku["kategori_adi"].ToString(), StringComparison.OrdinalIgnoreCase) || txtkategori.Text.Trim() == "")
                    {
                        durum = false;
                        break;
                    }
                }
                oku.Close();
            }
            finally
            {
                baglan.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            kategoriengelle();

            if (durum)
            {
                try
                {
                    baglan.Open();
                    MySqlCommand komut = new MySqlCommand("INSERT INTO kategoriler (kategori_adi) VALUES (@kategori)", baglan);
                    komut.Parameters.AddWithValue("@kategori", txtkategori.Text.Trim());
                    komut.ExecuteNonQuery();
                    MessageBox.Show("Kategori Eklendi");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata: " + ex.Message);
                }
                finally
                {
                    baglan.Close();
                }
            }
            else
            {
                MessageBox.Show("Girdiğiniz Kategori Zaten Mevcut", "UYARI!");
            }

            txtkategori.Text = "";
        }

        private void frmKategori_Load(object sender, EventArgs e)
        {
            // İstersen form yüklenince kategorileri listeleyebilirsin ama şu an gerek yok
        }

        private void label1_Click(object sender, EventArgs e)
        {
            // Gerek yok ama boş bırakılmış, çalışıyor
        }
    }
}
