using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace StokTakipOtomasyonu
{
    public partial class frmMusteriEkle : Form
    {
        MySqlConnection baglan = new MySqlConnection("Server=localhost;Database=stok_takip;Uid=root;Pwd=root;");

        public frmMusteriEkle()
        {
            InitializeComponent();
        }

        private void frmMusteriEkle_Load(object sender, EventArgs e)
        {
            // Yüklenince ekstra bir işlem yok
        }

        private void btnekle_Click(object sender, EventArgs e)
        {
            try
            {
                baglan.Open();
                MySqlCommand komut = new MySqlCommand(
                    "INSERT INTO musteriler (tc, adsoyad, telefon, adres, email) VALUES (@tc, @adsoyad, @telefon, @adres, @email)", baglan);
                komut.Parameters.AddWithValue("@tc", txttckimlik.Text.Trim());
                komut.Parameters.AddWithValue("@adsoyad", txtadsoyad.Text.Trim());
                komut.Parameters.AddWithValue("@telefon", txttelefon.Text.Trim());
                komut.Parameters.AddWithValue("@adres", txtadres.Text.Trim());
                komut.Parameters.AddWithValue("@email", txteposta.Text.Trim());
                komut.ExecuteNonQuery();
                baglan.Close();
                MessageBox.Show("Müşteri kaydı başarıyla eklendi!");
                Temizle();
            }
            catch (Exception ex)
            {
                baglan.Close();
                MessageBox.Show("Hata oluştu: " + ex.Message);
            }
        }

        private void btntemizle_Click(object sender, EventArgs e)
        {
            Temizle();
        }

        private void Temizle()
        {
            foreach (Control item in this.Controls)
            {
                if (item is TextBox)
                    item.Text = "";
            }
        }
    }
}
